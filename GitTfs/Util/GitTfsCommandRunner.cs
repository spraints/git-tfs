using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StructureMap;
using Sep.Git.Tfs.Commands;
using Sep.Git.Tfs.Core;
using System.Diagnostics;

namespace Sep.Git.Tfs.Util
{
    public class GitTfsCommandRunner
    {
        private readonly IHelpHelper _help;
        private readonly IContainer _container;
        private readonly TextWriter _stdout;

        public GitTfsCommandRunner(IHelpHelper help, IContainer container, TextWriter stdout)
        {
            _help = help;
            _container = container;
            _stdout = stdout;
        }

        public int Run(GitTfsCommand command, IList<string> args)
        {
            var returnValue = RunCommand(command, args);
            RunAfterFilters(command);
            return returnValue;
        }

        private int RunCommand(GitTfsCommand command, IList<string> args)
        {
            try
            {
                var runMethods = command.GetType().GetMethods().Where(m => m.Name == "Run" && m.ReturnType == typeof(int)).Select(m => new { Method = m, Parameters = m.GetParameters() });
                var splitRunMethods = runMethods.Where(m => m.Parameters.All(p => p.ParameterType == typeof(string)));
                var exactMatchingMethod = splitRunMethods.SingleOrDefault(m => m.Parameters.Length == args.Count);
                if (exactMatchingMethod != null)
                    return (int)exactMatchingMethod.Method.Invoke(command, args.ToArray());
                var defaultRunMethod = runMethods.FirstOrDefault(m => m.Parameters.Length == 1 && m.Parameters[0].ParameterType.IsAssignableFrom(args.GetType()));
                if (defaultRunMethod != null)
                    return (int)defaultRunMethod.Method.Invoke(command, new object[] { args });
                return _help.ShowHelpForInvalidArguments(command);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is GitTfsException)
                    throw ((GitTfsException) e.InnerException).ToRethrowable();
                throw;
            }
        }

        private void RunAfterFilters(GitTfsCommand command)
        {
            foreach (var attribute in command.GetType().GetCustomAttributes(typeof(AfterRunAttribute), true))
            {
                RunAfterFilter((AfterRunAttribute)attribute);
            }
        }

        private void RunAfterFilter(AfterRunAttribute attribute)
        {
            if (attribute.FilterClass != null)
            {
                var filter = (Filter)_container.GetInstance(attribute.FilterClass);
                try
                {
                    filter.Call();
                }
                catch (Exception e)
                {
                    _stdout.WriteLine("WARNING: after filter " + attribute.FilterClass.Name + " failed: " + e.Message);
                    Trace.WriteLine(e);
                }
            }
        }
    }
}
