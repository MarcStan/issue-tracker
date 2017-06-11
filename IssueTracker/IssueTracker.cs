using System;
using System.Collections.Generic;
using System.IO;

namespace IssueTracker
{
    /// <summary>
    /// Central logic for issue tracker.
    /// This is responsible for execution of the commands and will write directly to commandline on success/error.
    /// </summary>
    public class IssueTracker
    {
        private readonly string _workingDirectory;

        /// <summary>
        /// Creates a new instance that works in the provided directory.
        /// </summary>
        /// <param name="workingDirectory"></param>
        public IssueTracker(string workingDirectory)
        {
            if (string.IsNullOrEmpty(workingDirectory))
                throw new ArgumentNullException(nameof(workingDirectory));

            _workingDirectory = Path.GetFullPath(workingDirectory);
        }

        /// <summary>
        /// Gets whether the working directory is an issue tracker.
        /// </summary>
        public bool WorkingDirectoryIsIssueTracker => File.Exists(Path.Combine(_workingDirectory, ".issues"));

        /// <summary>
        /// Creates a new project in the current directory.
        /// </summary>
        public void InitializeNewProject()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Lists the issues that match the provided set of filters.
        /// </summary>
        /// <param name="filters"></param>
        public void ListIssues(List<FilterValue> filters)
        {
            throw new NotImplementedException();
        }
    }
}