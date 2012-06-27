using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sep.Git.Tfs.Commands;
using Sep.Git.Tfs.Core.TfsInterop;

namespace Sep.Git.Tfs.Core
{
    public class TfsWorkspace : ITfsWorkspace
    {
        private readonly IWorkspace _workspace;
        private readonly string _localDirectory;
        private readonly TextWriter _stdout;
        private readonly TfsChangesetInfo _contextVersion;
        private readonly IGitTfsRemote _remote;
        private readonly ITfsHelper _tfsHelper;
        private readonly CheckinPolicyEvaluator _policyEvaluator;

        public TfsWorkspace(IWorkspace workspace, string localDirectory, TextWriter stdout, TfsChangesetInfo contextVersion, IGitTfsRemote remote, ITfsHelper tfsHelper, CheckinPolicyEvaluator policyEvaluator)
        {
            _workspace = workspace;
            _policyEvaluator = policyEvaluator;
            _contextVersion = contextVersion;
            _remote = remote;
            _tfsHelper = tfsHelper;
            _localDirectory = localDirectory;
            _stdout = stdout;
        }

        public void Shelve(string shelvesetName, bool evaluateCheckinPolicies, bool force, string checkinComment)
        {
            var pendingChanges = _workspace.GetPendingChanges();

            if (pendingChanges.IsEmpty())
                throw new GitTfsException("Nothing to shelve!");

            var shelveset = _tfsHelper.CreateShelveset(_workspace, shelvesetName);
            shelveset.Comment = checkinComment;
            shelveset.WorkItemInfo = GetWorkItemInfos().ToArray();
            if(evaluateCheckinPolicies)
            {
                foreach(var message in _policyEvaluator.EvaluateCheckin(_workspace, pendingChanges, shelveset.Comment, shelveset.WorkItemInfo).Messages)
                {
                    _stdout.WriteLine("[Checkin Policy] " + message);
                }
            }
            _workspace.Shelve(shelveset, pendingChanges, force ? TfsShelvingOptions.Replace : TfsShelvingOptions.None);
        }

        public long CheckinTool(string checkinComment)
        {
            var pendingChanges = _workspace.GetPendingChanges();

            if (pendingChanges.IsEmpty())
                throw new GitTfsException("Nothing to checkin!");

            var newChangesetId = _tfsHelper.ShowCheckinDialog(_workspace, pendingChanges, GetWorkItemCheckedInfos(), checkinComment);
            if(newChangesetId <= 0)
                throw new GitTfsException("Checkin cancelled!");
            return newChangesetId;
        }

        public long Checkin(string policyOverrideReason, bool overrideGatedCheckin, string checkinComment)
        {
            var pendingChanges = _workspace.GetPendingChanges();

            if(pendingChanges.IsEmpty())
                throw new GitTfsException("Nothing to checkin!");

            var workItemInfos = GetWorkItemInfos();
            var checkinProblems = _policyEvaluator.EvaluateCheckin(_workspace, pendingChanges, checkinComment, workItemInfos);
            if(checkinProblems.HasErrors)
            {
                foreach (var message in checkinProblems.Messages)
                {
                    _stdout.WriteLine("[ERROR] " + message);
                }
                if (policyOverrideReason == null)
                {
                    throw new GitTfsException("No changes checked in.");
                }
                if (String.IsNullOrWhiteSpace(policyOverrideReason))
                {
                    throw new GitTfsException("A reason must be supplied (-f REASON) to override the policy violations.");
                }
            }

            var policyOverride = GetPolicyOverrides(checkinProblems.Result, policyOverrideReason);
            var newChangeset = _workspace.Checkin(pendingChanges, checkinComment, null, workItemInfos, policyOverride, overrideGatedCheckin);
            if(newChangeset == 0)
            {
                throw new GitTfsException("Checkin failed!");
            }
            else
            {
                return newChangeset;
            }
        }

        private TfsPolicyOverrideInfo GetPolicyOverrides(ICheckinEvaluationResult checkinProblems, string policyOverrideReason)
        {
            if (string.IsNullOrWhiteSpace(policyOverrideReason))
                return null;
            return new TfsPolicyOverrideInfo { Comment = policyOverrideReason, Failures = checkinProblems.PolicyFailures };
        }

        public string GetLocalPath(string path)
        {
            return Path.Combine(_localDirectory, path);
        }

        public void Add(string path)
        {
            _stdout.WriteLine(" add " + path);
            var added = _workspace.PendAdd(GetLocalPath(path));
            if (added != 1) throw new Exception("One item should have been added, but actually added " + added + " items.");
        }

        public void Edit(string path)
        {
            _stdout.WriteLine(" edit " + path);
            GetFromTfs(path);
            var edited = _workspace.PendEdit(GetLocalPath(path));
            if(edited != 1) throw new Exception("One item should have been edited, but actually edited " + edited + " items.");
        }

        public void Delete(string path)
        {
            _stdout.WriteLine(" delete " + path);
            GetFromTfs(path);
            var deleted = _workspace.PendDelete(GetLocalPath(path));
            if (deleted != 1) throw new Exception("One item should have been deleted, but actually deleted " + deleted + " items.");
        }

        public void Rename(string pathFrom, string pathTo, string score)
        {
            _stdout.WriteLine(" rename " + pathFrom + " to " + pathTo + " (score: " + score + ")");
            GetFromTfs(pathFrom);
            var result = _workspace.PendRename(GetLocalPath(pathFrom), GetLocalPath(pathTo));
            if (result != 1) throw new ApplicationException("Unable to rename item from " + pathFrom + " to " + pathTo);
        }

        private void GetFromTfs(string path)
        {
            _workspace.ForceGetFile(_remote.TfsRepositoryPath + "/" + path, (int) _contextVersion.ChangesetId);
        }

        private IEnumerable<IWorkItemCheckinInfo> GetWorkItemInfos()
        {
            return GetWorkItemInfosHelper<IWorkItemCheckinInfo>(_tfsHelper.GetWorkItemInfos);
        }

        private IEnumerable<IWorkItemCheckedInfo> GetWorkItemCheckedInfos()
        {
            return GetWorkItemInfosHelper<IWorkItemCheckedInfo>(_tfsHelper.GetWorkItemCheckedInfos);
        }

        private IEnumerable<T> xGetWorkItemInfosHelper<T>(Func<IEnumerable<string>, TfsWorkItemCheckinAction, IEnumerable<T>> func)
        {
            var workItemInfos = func(_checkinOptions.WorkItemsToAssociate, TfsWorkItemCheckinAction.Associate);
            workItemInfos = workItemInfos.Append(
                func(_checkinOptions.WorkItemsToResolve, TfsWorkItemCheckinAction.Resolve));
            return workItemInfos;
        }
    }
}
