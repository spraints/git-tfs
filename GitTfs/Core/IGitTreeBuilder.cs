using System;

namespace Sep.Git.Tfs.Core
{
    public interface IGitTreeBuilder
    {
        void Add(string path, string file, LibGit2Sharp.Mode mode);
        void Remove(string path);
        string GetTree();
    }

    public static class IGitTreeBuilderExt
    {
        public static void Add(this IGitTreeBuilder treeBuilder, string path, string file, string mode)
        {
            treeBuilder.Add(path, file, mode.ToFileMode());
        }
    }
}
