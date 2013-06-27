using System;
using Xunit;

namespace Sep.Git.Tfs.Test.Integration
{
    public class CheckinTests : IDisposable
    {
        IntegrationHelper h;

        public CheckinTests()
        {
            h = new IntegrationHelper();
        }

        public void Dispose()
        {
            h.Dispose();
        }

        [FactExceptOnUnix]
        public void ChecksIn()
        {
            throw new NotImplementedException("todo: test for `git tfs checkin`");
        }

        [FactExceptOnUnix]
        public void DoesAGatedCheckin()
        {
            throw new NotImplementedException("todo: test for `git tfs checkin` when gated checkins are required on the project");
        }
    }
}
