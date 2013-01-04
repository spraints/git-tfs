using System;
using Sep.Git.Tfs.Core.TfsInterop;
using Xunit;

namespace Sep.Git.Tfs.Test.Integration
{
    public class CheckinTests : IDisposable
    {
        IntegrationHelper h;

        public CheckinTests()
        {
            h = new IntegrationHelper();
            SetUpClonedRepo();
        }

        public void Dispose()
        {
            h.Dispose();
        }

        private void SetUpClonedRepo()
        {
            h.SetupFake(r =>
            {
                r.Changeset(1, "Project created from template", DateTime.Parse("2012-01-01 12:12:12 -05:00"))
                    .Change(TfsChangeType.Add, TfsItemType.Folder, "$/MyProject");
                r.Changeset(2, "First commit", DateTime.Parse("2012-01-02 12:12:12 -05:00"))
                    .Change(TfsChangeType.Add, TfsItemType.Folder, "$/MyProject/Folder")
                    .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/Folder/File.txt", "File contents")
                    .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/README", "tldr");
            });
            h.Run("clone", h.TfsUrl, "$/MyProject");
        }

        [FactExceptOnUnix]
        public void CheckinANewFile()
        {
            h.AddFileToIndex("MyProject", path: "NewFile.txt", content: "file contents");
            h.Commit("MyProject", message: "git commit message");
            h.RunIn("MyProject", "checkin", "-m", "tfs checkin message");
            var latestChangeset = h.GetLatestChangeset();
            Assert.Equal("tfs checkin message", latestChangeset.Comment);
            Assert.Equal(1, latestChangeset.Changes.Count);
            Assert.Equal(TfsChangeType.Add, latestChangeset.Changes[0].ChangeType);
            Assert.Equal("$/MyProject/NewFile.txt", latestChangeset.Changes[0].RepositoryPath);
            Assert.Equal("file contents", latestChangeset.Changes[0].Content);
        }
    }
}
