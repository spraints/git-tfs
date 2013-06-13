using System;

namespace GitTfs.Profiling
{
    class NullProfiler : Profiler
    {
        public override void Sample(string sampleName)
        {
            // NOTHING
        }
    }
}
