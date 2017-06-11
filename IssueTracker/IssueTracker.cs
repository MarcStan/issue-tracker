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
        /// Returns the current user name as found in the .gitconfig.
        /// </summary>
        public virtual string CurrentUser => GitHelper.GetUserName();

        /// <summary>
        /// Gets whether the working directory is an issue tracker.
        /// </summary>
        public virtual bool WorkingDirectoryIsIssueTracker => File.Exists(Path.Combine(_workingDirectory, ".issues"));

        /// <summary>
        /// Creates a new project in the current directory.
        /// </summary>
        public virtual void InitializeNewProject()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Lists the issues that match the provided set of filters.
        /// </summary>
        /// <param name="filters"></param>
        public virtual void ListIssues(List<FilterValue> filters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reopens the isse with the provided id (if found and closed).
        /// </summary>
        /// <param name="id"></param>
        public virtual void ReopenIssue(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reopens the isse with the provided id (if found and open).
        /// </summary>
        /// <param name="id"></param>
        public virtual void CloseIssue(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Displays the provided issue with all its comments.
        /// </summary>
        /// <param name="id"></param>
        public virtual void ShowIssue(int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a new issue to the issue tracker.
        /// </summary>
        /// <param name="title">Required.</param>
        /// <param name="message">Optional.</param>
        /// <param name="tags">Optional.</param>
        public virtual void AddIssue(string title, string message, Tag[] tags)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Edits the tags of the issue with the provided id (if found).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="add"></param>
        /// <param name="remove"></param>
        public virtual void EditTags(int id, Tag[] add, Tag[] remove)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Comments on the provided issue.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        public virtual void CommentIssue(int id, string message)
        {
            throw new NotImplementedException();
        }
    }
}