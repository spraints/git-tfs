using System;

namespace GitTfs.Profiling
{
    public class ProfilerLoader
    {
        public Profiler Instance { get; private set; }

        public ProfilerLoader()
        {
            Instance = new NullProfiler();
        }

        public void Init(string profilerSpec)
        {
            throw new Exception("todo: init profiler");
        }
    }
}
