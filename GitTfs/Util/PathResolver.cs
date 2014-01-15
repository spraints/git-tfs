using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sep.Git.Tfs.Core;

namespace Sep.Git.Tfs.Util
{
    public class PathResolver
    {
        IGitTfsRemote _remote;
        IGitTreeInformation _initialTree;

        public PathResolver(IGitTfsRemote remote, IGitTreeInformation initialTree)
        {
            _remote = remote;
            _initialTree = initialTree;
        }

        public string GetPathInGitRepo(string tfsPath)
        {
            return _remote.GetPathInGitRepo(tfsPath);
        }

        public class ResolvedItem
        {
            public string Path { get; set; }
            public string Mode { get; set; }
        }

        public ResolvedItem Resolve(string tfsPath)
        {
            var pathInGitRepo = _remote.GetPathInGitRepo(tfsPath);
            if (string.IsNullOrEmpty(pathInGitRepo))
                return null;
            return Lookup(pathInGitRepo);
        }

        public bool ShouldIncludeGitItem(string gitPath)
        {
            return !String.IsNullOrEmpty(gitPath) && !_remote.ShouldSkip(gitPath);
        }

        private ResolvedItem Lookup(string pathInGitRepo)
        {
            return new ResolvedItem
            {
                Path = pathInGitRepo,
                Mode = _initialTree.GetMode(pathInGitRepo),
            };
        }
    }
}
