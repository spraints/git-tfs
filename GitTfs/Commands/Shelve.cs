using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NDesk.Options;
using Sep.Git.Tfs.Core;
using Sep.Git.Tfs.Util;
using StructureMap;

namespace Sep.Git.Tfs.Commands
{
    [Pluggable("shelve")]
    [Description("shelve [options] shelveset-name [ref-to-shelve]")]
    [RequiresValidGitRepository]
    public class Shelve : GitTfsCommand
    {
        private readonly TextWriter _stdout;
        private readonly CheckinOptions _checkinOptions;
        private readonly TfsWriter _writer;
        private readonly Commenter _commenter;

        private bool EvaluateCheckinPolicies { get; set; }

        public Shelve(TextWriter stdout, CheckinOptions checkinOptions, TfsWriter writer, Commenter commenter)
        {
            _stdout = stdout;
            _checkinOptions = checkinOptions;
            _writer = writer;
            _commenter = commenter;
        }

        public OptionSet OptionSet
        {
            get
            {
                return new OptionSet
                {
                    { "p|evaluate-policies", "Evaluate checkin policies (default: false)",
                        v => EvaluateCheckinPolicies = v != null },
                    { "f|force", "Force a shelve, and overwrite an existing shelveset",
                        v => { this._checkinOptions.Force = true; } },
                }.Merge(_checkinOptions.OptionSet);
            }
        }

        public int Run(string shelvesetName)
        {
            return Run(shelvesetName, "HEAD");
        }

        public int Run(string shelvesetName, string refToShelve)
        {
            return _writer.Write(refToShelve, changeset =>
            {
                if (!_checkinOptions.Force && changeset.Remote.HasShelveset(shelvesetName))
                {
                    _stdout.WriteLine("Shelveset \"" + shelvesetName + "\" already exists. Use -f to replace it.");
                    return GitTfsExitCodes.ForceRequired;
                }
                changeset.Remote.Shelve(shelvesetName, refToShelve, changeset, EvaluateCheckinPolicies, _commenter.Comment(changeset.Remote.Repository, refToShelve, changeset.GitCommit));
                return GitTfsExitCodes.OK;
            });
        }
    }
}
