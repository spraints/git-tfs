using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Sep.Git.Tfs.Core;
using Sep.Git.Tfs.Core.Changes.Git;
using Sep.Git.Tfs.Core.TfsInterop;
using Sep.Git.Tfs.Util;
using StructureMap;
using StructureMap.Graph;



/*
 * TODO: 
 * 1. Fix up build configuration.
 *    * GitTfsTest is always enabled.
 *    * VS2008 is x86 (so git-tfs should be, too, and git-tfs-test)
 *    * Other plugins should be any cpu
 *    * integrate the vsXXtest project, via VsFake?
 *    * other plugins are disabled by default?
 * 2. Init:
 *    * [fc] input = command-line flags, output = config vars to set
 *    * [fc] input = config vars to set, output = errors / warnings (e.g. tfs id is already configured)
 * 3. Fetch
 *    * [fc] input = command-line flags; output = current config (or multiple configs) to fetch
 *    * [fc] input = command-line flags, current config, git history; output = starting point (null or a changeset / commit, and a branch to update)
 *    * [fc] input = changeset info, current tree; output = tree mods, in order
 * 4. quick-fetch
 *    * ?
 * 5. checkin / shelve / checkintool
 *    * ?
 * 6. rcheckin
 *    * ?
 * 7. pull (fetch plus...)
 *    * [fc] input = status of head, starting point (from fetch); output = go / nogo
 */



 
namespace Sep.Git.Tfs
{
    public class Program
    {
        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            try
            {
                //Trace.Listeners.Add(new ConsoleTraceListener());
                var container = Initialize();
                container.GetInstance<GitTfs>().Run(new List<string>(args));
            }
            catch(GitTfsException e)
            {
                Trace.WriteLine(e);
                Console.WriteLine(e.Message);
                if (!e.RecommendedSolutions.IsEmpty())
                {
                    Console.WriteLine("You may be able to resolve this problem.");
                    foreach (var solution in e.RecommendedSolutions)
                    {
                        Console.WriteLine("- " + solution);
                    }
                }
                Environment.ExitCode = GitTfsExitCodes.ExceptionThrown;
            }
            catch (Exception e)
            {
                ReportException(e);
                Environment.ExitCode = GitTfsExitCodes.ExceptionThrown;
            }
        }

        private static void ReportException(Exception e)
        {
            Trace.WriteLine(e);
            while(e is TargetInvocationException && e.InnerException != null)
                e = e.InnerException;
            while (e != null)
            {
                Console.WriteLine(e.Message);
                e = e.InnerException;
            }
        }

        private static IContainer Initialize()
        {
            return new Container(Initialize);
        }

        private static void Initialize(ConfigurationExpression initializer)
        {
            var tfsPlugin = TfsPlugin.Find();
            initializer.Scan(x => { Initialize(x); tfsPlugin.Initialize(x); });
            initializer.For<TextWriter>().Use(() => Console.Out);
            initializer.For<IGitRepository>().Add<GitRepository>();
            AddGitChangeTypes(initializer);
            DoCustomConfiguration(initializer);
            tfsPlugin.Initialize(initializer);
        }

        public static void AddGitChangeTypes(ConfigurationExpression initializer)
        {
            // See git-diff-tree(1).
            initializer.For<IGitChangedFile>().Use<Add>().Named("A");
            initializer.For<IGitChangedFile>().Use<Copy>().Named("C");
            initializer.For<IGitChangedFile>().Use<Modify>().Named("M");
            //initializer.For<IGitChangedFile>().Use<TypeChange>().Named("T");
            initializer.For<IGitChangedFile>().Use<Delete>().Named("D");
            initializer.For<IGitChangedFile>().Use<RenameEdit>().Named("R");
            //initializer.For<IGitChangedFile>().Use<Unmerged>().Named("U");
            //initializer.For<IGitChangedFile>().Use<Unknown>().Named("X");
        }

        private static void Initialize(IAssemblyScanner scan)
        {
            scan.WithDefaultConventions();
            scan.TheCallingAssembly();
        }

        private static void DoCustomConfiguration(ConfigurationExpression initializer)
        {
            foreach(var type in typeof(Program).Assembly.GetTypes())
            {
                foreach(ConfiguresStructureMap attribute in type.GetCustomAttributes(typeof(ConfiguresStructureMap), false))
                {
                    attribute.Initialize(initializer, type);
                }
            }
        }
    }
}
