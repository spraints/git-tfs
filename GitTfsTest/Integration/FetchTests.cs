﻿using System;
using LibGit2Sharp;
using Sep.Git.Tfs.Core.TfsInterop;
using Xunit;

namespace Sep.Git.Tfs.Test.Integration
{
    public class FetchTests : IDisposable
    {
        private readonly IntegrationHelper integrationHelper;

        public FetchTests()
        {
            integrationHelper = new IntegrationHelper();
        }

        public void Dispose()
        {
            integrationHelper.Dispose();
        }

        [FactExceptOnUnix]
        public void CanFetchWithMixedUpCasingForTfsServerUrl()
        {
            CloneRepoWithTwoCommits();
            AddNewCommitToFakeTfsServer();
            string tfsUrlInUpperCase = integrationHelper.TfsUrl.ToUpper();
            integrationHelper.ChangeConfigSetting("MyProject", "tfs-remote.default.url", tfsUrlInUpperCase);

            integrationHelper.RunIn("MyProject", "pull");

            Assert.Equal(3, integrationHelper.GetCommitCount("MyProject"));
        }

        [FactExceptOnUnix]
        public void CanFetchWithMixedUpCasingForLegacyTfsServerUrl()
        {
            CloneRepoWithTwoCommits();
            AddNewCommitToFakeTfsServer();
            string tfsUrlInUpperCase = integrationHelper.TfsUrl.ToUpper();
            integrationHelper.ChangeConfigSetting("MyProject", "tfs-remote.default.url", "nomatch");
            integrationHelper.ChangeConfigSetting("MyProject", "tfs-remote.default.legacy-urls", tfsUrlInUpperCase + ",aDifferentUrl");

            integrationHelper.RunIn("MyProject", "pull");

            Assert.Equal(3, integrationHelper.GetCommitCount("MyProject"));
        }

        [FactExceptOnUnix]
        public void CanFetchWithMixedUpCasingForTfsRepositoryPath()
        {
            CloneRepoWithTwoCommits();
            AddNewCommitToFakeTfsServer();
            const string repoUrlInUpperCase = "$/MYPROJECT";
            integrationHelper.ChangeConfigSetting("MyProject", "tfs-remote.default.repository", repoUrlInUpperCase);

            integrationHelper.RunIn("MyProject", "pull");

            Assert.Equal(3, integrationHelper.GetCommitCount("MyProject"));
        }

        [FactExceptOnUnix]
        public void AdvancesToTfsHead()
        {
            Commit startTfsHead = null;
            Commit remoteTfsHead = null;
            integrationHelper.SetupGitRepo("MyProject", g =>
            {
                startTfsHead = g.Commit("Changeset 1.\n\ngit-tfs-id: [http://server/tfs]$/MyProject;C1");
                g.Ref("refs/remote/tfs/default", startTfsHead);

                remoteTfsHead = g.Commit("Changeset 2.\n\ngit-tfs-id: [http://server/tfs]$/MyProject;C2", parentCommit: startTfsHead);
                g.Ref("refs/remote/origin/master", remoteTfsHead);

                var localHead = g.Commit("Local commit 1.", parentCommit: startTfsHead);
                g.Ref("refs/heads/master", localHead);
            });
            integrationHelper.SetupFake(r =>
            {
                r.Changeset(1, "Changeset 1.")
                 .Change(TfsChangeType.Add, TfsItemType.Folder, "$/MyProject")
                 .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/README.txt, Changeset 1.");
                r.Changeset(2, "Changeset 2, but this one isn't used.")
                 .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/README.txt", "Changeset 2.");
            });
            integrationHelper.RunIn("MyProject", "init", "http://server/tfs");
            integrationHelper.AssertRef("MyProject", "refs/remote/tfs/default", startTfsHead.Sha);
            integrationHelper.RunIn("MyProject", "fetch");
            integrationHelper.AssertRef("MyProject", "refs/remote/tfs/default", remoteTfsHead.Sha);
        }

        private void CloneRepoWithTwoCommits()
        {
            integrationHelper.SetupFake(r =>
                                            {
                                                r.Changeset(1, "Project created from template", DateTime.Parse("2012-01-01 12:12:12 -05:00"))
                                                 .Change(TfsChangeType.Add, TfsItemType.Folder, "$/MyProject");
                                                r.Changeset(2, "Add Readme", DateTime.Parse("2012-01-02 12:12:12 -05:00"))
                                                 .Change(TfsChangeType.Add, TfsItemType.Folder, "$/MyProject/Folder")
                                                 .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/Folder/File.txt", "File contents")
                                                 .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/README", "tldr");
                                            });
            integrationHelper.Run("clone", integrationHelper.TfsUrl, "$/MyProject");
            integrationHelper.AssertGitRepo("MyProject");
        }

        private void AddNewCommitToFakeTfsServer()
        {
            integrationHelper.SetupFake(r => CreateAChangeset(r));
        }

        private static IntegrationHelper.FakeChangesetBuilder CreateAChangeset(IntegrationHelper.FakeHistoryBuilder r)
        {
            return r.Changeset(3, "Add a file", DateTime.Parse("2012-01-03 12:12:12 -05:00"))
                    .Change(TfsChangeType.Add, TfsItemType.Folder, "$/MyProject/Foo")
                    .Change(TfsChangeType.Add, TfsItemType.Folder, "$/MyProject/Foo/Bar")
                    .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/Foo/Bar/File.txt", "File contents");
        }
    }
}