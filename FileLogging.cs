using System;
using System.Diagnostics;
using System.IO;

namespace PackageRunner
{
    /// <summary>
    /// </summary>
    internal class FileLogging
    {
        private readonly string _file;
        private static readonly object _lock = new object();

        public FileLogging(string file)
        {
            _file = file;
        }

        public void AddLine(string text)
        {
            lock (_lock)
            {
                File.AppendAllText(_file, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + Environment.NewLine);
                Debug.WriteLine(text);
            }
        }

        /// <summary>
        /// </summary>
        public void AddLine(string format, params object[] args)
        {
            var text = string.Format(format, args);
            this.AddLine(text);
        }
    }
}
