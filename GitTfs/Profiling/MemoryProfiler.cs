using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using StructureMap;

namespace GitTfs.Profiling
{
    [Pluggable("memory")]
    public class MemoryProfiler : Profiler
    {
        string _currentSample = "";
        Thread _thread;
        bool _done;

        public MemoryProfiler()
        {
            _thread = new Thread(() => RunPoller());
            _thread.Start();
        }

        public override void Dispose()
        {
            if (!_done)
            {
                _done = true;
                System.Diagnostics.Trace.WriteLine("Waiting for profiler thread...");
                if (!_thread.Join(TimeSpan.FromSeconds(2)))
                {
                    System.Diagnostics.Trace.WriteLine("Killing the thread and moving on.");
                    _thread.Abort();
                }
            }
            base.Dispose();
        }

        void RunPoller()
        {
            while (!_done)
            {
                WriteRow(_currentSample, GetValues());
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }

        public override void Sample(string sampleName)
        {
            _currentSample = sampleName;
        }

        protected override IEnumerable GetValues()
        {
            return Counters.Select(counter => counter.NextValue());
        }

        protected override IEnumerable<string> GetColumnNames()
        {
            return Counters.Select(counter => counter.CounterName);
        }

        PerformanceCounter[] _counters;
        IEnumerable<PerformanceCounter> Counters
        {
            get
            {
                if (_counters == null)
                {
                    var category = new PerformanceCounterCategory(".NET CLR Memory");
                    _counters = category.GetCounters("git-tfs");
                }
                return _counters;
            }
        }
    }
}
