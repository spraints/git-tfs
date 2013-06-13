using System;
using System.Diagnostics;
using System.Linq;
using Sep.Git.Tfs.Core;
using Sep.Git.Tfs.Util;
using StructureMap;

namespace GitTfs.Profiling
{
    public class ProfilerLoader
    {
        public Profiler Instance { get; private set; }

        IContainer _container;

        public ProfilerLoader()
        {
            Instance = new NullProfiler();
        }

        public ProfilerLoader(IContainer container)
            : this()
        {
            _container = container;
        }

        public void Init(string profilerSpec)
        {
            try
            {
                throw new Exception("todo: init profiler");
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                var profilerPlugins = _container.GetPlugins<Profiler>();
                throw new GitTfsException("Unable to set up profiler \"" + profilerSpec + "\": " + e.Message +
                    "\nAvailable profilers:\n" + string.Join("\n", profilerPlugins.Select(p => "- " + p.Name)));
            }
        }
    }
}
