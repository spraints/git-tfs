using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StructureMap;

namespace GitTfs.Profiling
{
    [PluginFamily]
    public abstract class Profiler : IDisposable
    {
        TextWriter _writer;

        void InitWriter()
        {
            if (_writer == null)
            {
                var outputFile = string.Format("git-tfs-profile-{0:yyyyMMdd-hhmmss}.csv", DateTime.Now);
                var outputPath = Path.Combine(Environment.CurrentDirectory, outputFile);
                _writer = new StreamWriter(outputPath);
                WriteRow("", GetColumnNames());
            }
        }

        public virtual void Dispose()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Dispose();
                _writer = null;
            }
        }

        public virtual void Sample(string sampleName)
        {
            WriteRow(sampleName, GetValues());
        }

        protected virtual IEnumerable GetValues()
        {
            throw new NotImplementedException();
        }

        protected virtual IEnumerable<string> GetColumnNames()
        {
            throw new NotImplementedException();
        }

        void WriteRow(string col1, IEnumerable cols)
        {
            InitWriter();
            _writer.Write(col1);
            foreach(var col in cols)
            {
                _writer.Write(", ");
                _writer.Write(col);
            }
            _writer.WriteLine();
        }
    }
}
