﻿using IssueTracker.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IssueTracker
{
    /// <summary>
    /// Central logic for issue tracker.
    /// This is responsible for execution of the commands and will write directly to commandline on success/error.
    /// </summary>
    public class IssueTracker
    {
        private readonly string _workingDirectory;
        private const string RootFile = ".issues";

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
        public virtual bool WorkingDirectoryIsIssueTracker => File.Exists(Path.Combine(_workingDirectory, RootFile));

        /// <summary>
        /// Creates a new project in the current directory.
        /// </summary>
        public virtual void InitializeNewProject()
        {
            if (WorkingDirectoryIsIssueTracker)
            {
                Console.WriteLine("Already an issue tracking directory!");
                return;
            }
            var user = CurrentUser;
            File.WriteAllText(RootFile, $"owner={user}");

            Console.WriteLine($"New issue project created for owner {user}!");
        }

        /// <summary>
        /// Lists the issues that match the provided set of filters.
        /// </summary>
        /// <param name="filters"></param>
        public virtual void ListIssues(List<FilterValue> filters)
        {
            AssertIssueTracker();

            var issues = Storage.LoadIssues();
            bool stateWasFiltered = false;
            foreach (var f in filters)
            {
                if (f.FilterType == Filter.IssueState)
                    stateWasFiltered = true;

                ApplyFilter(f, issues);
            }
            if (!issues.Any())
            {
                Console.WriteLine("Found no matching issues!");
                return;
            }
            Console.WriteLine($"Found {issues.Count} matching issues:");
            // if a statefilter exists, only the specific state will be displayed -> no point in printing it again in each line
            // the user will only care if the issues are mixed (open and closed isses)
            PrintIssueList(issues, stateWasFiltered);
        }

        /// <summary>
        /// Prints the list of issues in a human readable fashion.
        /// </summary>
        /// <param name="issues"></param>
        /// <param name="displayState">If true the state (open/closed) of each issue will be displayed.</param>
        private static void PrintIssueList(List<Issue> issues, bool displayState)
        {
            var formatted = new List<string[]>();
            foreach (var i in issues)
            {
                var tagList = i.Tags.Length > 0 ? "[" + string.Join(", ", i.Tags.Select(t => t.Name)) + "]" : "";

                var cc = i.Comments.Count(c => c.Editable);
                var comments = cc > 0 ? $" ({cc} comments)" : "";
                formatted.Add(new[]
                {
                    $"#{i.Id}",
                    displayState ? $"[{i.State}]" : "",
                    $"'{i.Title}'",
                    "created by " + i.Author,
                    $"@ {i.CreationDate.ToShortDateString()}",
                    comments,
                    tagList
                });
            }
            Console.WriteLine(ConsoleFormatter.PadElementsInLines(formatted));
        }

        /// <summary>
        /// Applies the specific filter on the collection, removing any items that don't match
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="issues"></param>
        private static void ApplyFilter(FilterValue filter, List<Issue> issues)
        {
            switch (filter.FilterType)
            {
                case Filter.Tag:
                    var tags = (Tag[])filter.Value;
                    issues.RemoveAll(i => i.Tags.ContainsAll(tags));
                    break;
                case Filter.IssueState:
                    var s = (IssueState)filter.Value;
                    issues.RemoveAll(i =>
                    {
                        // remove all open issues; either open or reopened
                        if ((s == IssueState.Open || s == IssueState.Reopened) &&
                            (i.State == IssueState.Open || i.State == IssueState.Reopened))
                        {
                            return true;
                        }

                        // remove closed
                        return s == IssueState.Closed && i.State == IssueState.Closed;
                    });
                    break;
                case Filter.User:
                    var user = filter.Value.ToString();
                    issues.RemoveAll(i => i.Author.Equals(user, StringComparison.InvariantCultureIgnoreCase));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void AssertIssueTracker()
        {
            if (!WorkingDirectoryIsIssueTracker)
                throw new NotSupportedException("Command must execute in a issue tracker directory");
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