using System;
using StructureMap;
using System.Diagnostics;

namespace GitTfs.Profiling
{
    /// <summary>
    /// Let you use an external sampling profiler.
    /// </summary>
    [Pluggable("external")]
    public class PausingProfiler : Profiler
    {
        public override void Sample(string sampleName)
        {
            Console.WriteLine("Paused PID " + Process.GetCurrentProcess().Id + " for profiling. Press <Enter> to continue.");
            Console.ReadLine();
        }
    }
}
