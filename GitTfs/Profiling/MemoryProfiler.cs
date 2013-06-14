using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StructureMap;

namespace GitTfs.Profiling
{
    [Pluggable("memory")]
    public class MemoryProfiler : Profiler
    {
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
