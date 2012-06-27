using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sep.Git.Tfs.Commands;
using Sep.Git.Tfs.Core;

namespace Sep.Git.Tfs.Util
{
    public class Commenter
    {
        CheckinOptions _checkinOptions;

        public Commenter(CheckinOptions checkinOptions)
        {
            _checkinOptions = checkinOptions;
        }

        public string Comment(IGitRepository repository, string head, string mergeBase)
        {
            if (String.IsNullOrWhiteSpace(_checkinOptions.CheckinComment) && !_checkinOptions.NoGenerateCheckinComment)
            {
                return repository.GetCommitMessage(head, mergeBase);
            }

            return _checkinOptions.CheckinComment;
        }
    }
}
