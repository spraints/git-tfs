using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Sep.Git.Tfs.Test.Integration
{
    public class IntegrationTests
    {
        protected IntegrationHelper h;

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

    }
}
