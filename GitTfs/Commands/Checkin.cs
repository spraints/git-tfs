using System.ComponentModel;
using System.IO;
using Sep.Git.Tfs.Core;
using StructureMap;

namespace Sep.Git.Tfs.Commands
{
    [Pluggable("checkin")]
    [Description("checkin [options] [ref-to-shelve]")]
    [RequiresValidGitRepository]
    public class Checkin : CheckinBase
    {
        public Checkin(TextWriter stdout, CheckinOptions checkinOptions, TfsWriter writer)
            : base(stdout, checkinOptions, writer)
        {
        }

        protected override long DoCheckin(TfsChangesetInfo changeset, string refToCheckin)
        {
            if (changeset.Remote.GatedCheckinsRequired || _checkinOptions.Gated)
            {
                changeset.Remote.Shelve("todo", refToCheckin, changeset, true);
                return GitTfsExitCodes.OK;
            }
            return changeset.Remote.Checkin(refToCheckin, changeset, _checkinOptions);
        }
    }
}
