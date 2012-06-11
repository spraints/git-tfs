using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sep.Git.Tfs.Core.TfsInterop;

namespace Sep.Git.Tfs.Test.Integration
{
    [TestClass]
    public class CloneTests
    {
        IntegrationHelper h;

        [TestInitialize]
        public void Setup()
        {
            h = new IntegrationHelper();
        }

        [TestCleanup]
        public void Teardown()
        {
            h.Dispose();
        }

        [TestMethod, Ignore]
        public void FailOnNoProject()
        {
        }

        [TestMethod, Ignore]
        public void ClonesEmptyProject()
        {
            h.SetupFake(r =>
            {
                r.CreateProject(1, DateTime.Parse("2012-01-01 12:12:12"), "$/MyProject");
            });
            h.Run("clone", h.TfsUrl, "$/MyProject");
            h.AssertGitRepo("MyProject");
            const string expectedSha = "tbd";
            h.AssertRef("MyProject", "HEAD", expectedSha);
            h.AssertRef("MyProject", "master", expectedSha);
            h.AssertRef("MyProject", "tfs/default", expectedSha);
            h.AssertEmptyWorkspace("MyProject");
        }

        [TestMethod]
        public void CloneProjectWithChangesets()
        {
            h.SetupFake(r =>
            {
                r.CreateProject(1, DateTime.Parse("2012-01-01 12:12:12"), "$/MyProject");
                r.Changeset(2, "First commit", DateTime.Parse("2012-01-02 12:12:12"))
                    .Change(TfsChangeType.Add, TfsItemType.Folder, "$/MyProject/Folder")
                    .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/Folder/File.txt", "File contents")
                    .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/README", "tldr");
            });
            h.Run("clone", h.TfsUrl, "$/MyProject");
            h.AssertGitRepo("MyProject");
            const string expectedSha = "dd806911118e6fa16d028b322ad91360d56ea47b";
            h.AssertRef("MyProject", "HEAD", expectedSha);
            h.AssertRef("MyProject", "master", expectedSha);
            h.AssertRef("MyProject", "tfs/default", expectedSha);
            h.AssertFileInWorkspace("MyProject", "Folder/File.txt", "File contents");
            h.AssertFileInWorkspace("MyProject", "README", "tldr");
        }

        [TestMethod]
        public void CloneProjectWithMixedNewlines()
        {
            var dosNewlines = "This\r\nHas\r\nDos\r\nNewlines\r\n";
            var unixNewlines = "This\nHas\nUnix\nNewlines\n";
            var mixedNewlines = "This\r\nHas\nMixed\rNewlines\n";
            h.SetupFake(r =>
            {
                r.CreateProject(1, DateTime.Parse("2012-01-01 12:12:12"), "$/MyProject");
                r.Changeset(2, "First commit", DateTime.Parse("2012-01-02 12:12:12"))
                    .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/dos.txt", dosNewlines)
                    .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/unix.txt", unixNewlines)
                    .Change(TfsChangeType.Add, TfsItemType.File, "$/MyProject/mixed.txt", mixedNewlines);
            });
            h.Run("clone", h.TfsUrl, "$/MyProject");
            h.AssertFileInWorkspace("MyProject", "dos.txt", dosNewlines);
            h.AssertFileInWorkspace("MyProject", "unix.txt", unixNewlines);
            h.AssertFileInWorkspace("MyProject", "mixed.txt", mixedNewlines);
        }
    }
}
