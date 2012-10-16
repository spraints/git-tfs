using System;
using System.Collections.Generic;
using LibGit2Sharp;
using Sep.Git.Tfs.Commands;

namespace Sep.Git.Tfs.Core
{
    public class RemoteConfigInitializer
    {
        public IEnumerable<ConfigurationEntry> ConfigFor(string remoteId, string tfsUrl, string tfsRepositoryPath, RemoteOptions remoteOptions)
        {
            throw new NotImplementedException();
        }
    }
}
