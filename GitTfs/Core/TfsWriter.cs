using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sep.Git.Tfs.Core
{
    public class TfsWriter
    {
        private readonly TextWriter _stdout;
        private readonly Globals _globals;

        public TfsWriter(TextWriter stdout, Globals globals)
        {
            _stdout = stdout;
            _globals = globals;
        }

        /// <summary>
        /// Locates the base changeset/commit to use when checking in or shelving `refToWrite`.
        /// </summary>
        [Obsolete("This is a dumb API. Use FindBaseChangesetCommit instead.")]
        public int Write(string refToWrite, Func<TfsChangesetInfo, string, int> write)
        {
            var parentChangeset = FindParentChangeset(refToWrite);
            if (parentChangeset != null)
                return write(parentChangeset, refToWrite);
            return GitTfsExitCodes.InvalidArguments;
        }

        /// <summary>
        /// Locates the base changeset/commit to use when checking in or shelving `refToWrite`.
        /// </summary>
        private TfsChangesetInfo FindParentChangeset(string gitRef)
        {
            var tfsParents = _globals.Repository.GetLastParentTfsCommits(gitRef);
            if (_globals.UserSpecifiedRemoteId != null)
                tfsParents = tfsParents.Where(changeset => changeset.Remote.Id == _globals.UserSpecifiedRemoteId);
            switch (tfsParents.Count())
            {
                case 1:
                    return tfsParents.First();
                case 0:
                    _stdout.WriteLine("No TFS parents found!");
                    return null;
                default:
                    _stdout.WriteLine("More than one parent found! Use -i to choose the correct parent from: ");
                    foreach (var parent in tfsParents)
                    {
                        _stdout.WriteLine("  " + parent.Remote.Id);
                    }
                    return null;
            }
        }
    }
}
