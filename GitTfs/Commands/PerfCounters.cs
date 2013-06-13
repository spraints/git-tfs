using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using StructureMap;

namespace Sep.Git.Tfs.Commands
{
    [Pluggable("perf-counters")]
    public class PerfCounters : GitTfsCommand
    {
        TextWriter _stdout;

        public PerfCounters(TextWriter stdout)
        {
            _stdout = stdout;
        }

        public NDesk.Options.OptionSet OptionSet
        {
            get { return new NDesk.Options.OptionSet(); }
        }

        public int Run()
        {
            List("Categories", PerformanceCounterCategory.GetCategories(),
                category => category.CategoryName);
            return GitTfsExitCodes.OK;
        }

        public int Run(string categoryName)
        {
            var category = GetCategory(categoryName);
            switch (category.CategoryType)
            {
                case PerformanceCounterCategoryType.SingleInstance:
                    List(categoryName + " Counters", category.GetCounters());
                    break;
                case PerformanceCounterCategoryType.MultiInstance:
                    List(categoryName + " Instances", category.GetInstanceNames());
                    break;
                default:
                    _stdout.WriteLine("Don't know how to handle category type " + category.CategoryType);
                    return GitTfsExitCodes.InvalidArguments;
            }
            return GitTfsExitCodes.OK;
        }

        public int Run(string categoryName, string instanceName)
        {
            var category = GetCategory(categoryName);
            List(categoryName + " / " + instanceName, category.GetCounters(instanceName));
            return GitTfsExitCodes.OK;
        }

        private PerformanceCounterCategory GetCategory(string categoryName)
        {
            return PerformanceCounterCategory.GetCategories().Single(c => c.CategoryName == categoryName);
        }

        private void List(string label, IEnumerable<PerformanceCounter> counters)
        {
            List(label, counters, counter => counter.CounterName + " - " + counter.NextValue());
        }

        private void List<Thing>(string label, IEnumerable<Thing> things)
        {
            List(label, things, x => x.ToString());
        }

        private void List<Thing>(string label, IEnumerable<Thing> things, Func<Thing, string> format)
        {
            _stdout.WriteLine(label);
            foreach (var s in things.Select(x => format(x)).OrderBy(s => s))
            {
                _stdout.WriteLine("- " + s);
            }
        }
    }
}
