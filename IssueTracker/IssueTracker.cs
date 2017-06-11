using IssueTracker.Extensions;
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
            ChangeIssueState(id, IssueState.Reopened);
        }

        /// <summary>
        /// Reopens the isse with the provided id (if found and open).
        /// </summary>
        /// <param name="id"></param>
        public virtual void CloseIssue(int id)
        {
            ChangeIssueState(id, IssueState.Closed);
        }

        /// <summary>
        /// Displays the provided issue with all its comments.
        /// </summary>
        /// <param name="id"></param>
        public virtual void ShowIssue(int id)
        {
            var issues = Storage.LoadIssues();
            var issue = issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                Console.WriteLine($"No issue with id '#{id}' found!");
                return;
            }
            DisplayIssue(issue);
        }

        /// <summary>
        /// Adds a new issue to the issue tracker.
        /// </summary>
        /// <param name="title">Required.</param>
        /// <param name="message">Optional.</param>
        /// <param name="tags">Optional.</param>
        public virtual void AddIssue(string title, string message, Tag[] tags)
        {
            AssertIssueTracker();

            var all = Storage.LoadIssues();
            var maxId = all.Any() ? all.Max(i => i.Id) : 0;
            var issue = new Issue(maxId + 1, title, message, tags, DateTime.Now, CurrentUser, null, IssueState.Open, -1);
            Storage.SaveIssue(issue, true);
            Console.WriteLine($"Created issue '#{issue.Id}' {issue.Title}");
        }

        /// <summary>
        /// Edits the tags of the issue with the provided id (if found).
        /// </summary>
        /// <param name="id"></param>
        /// <param name="add"></param>
        /// <param name="remove"></param>
        public virtual void EditTags(int id, Tag[] add, Tag[] remove)
        {
            AssertIssueTracker();

            var issues = Storage.LoadIssues();
            var issue = issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                Console.WriteLine($"No issue with id '#{id}' found!");
                return;
            }

            if (add.Any(remove.Contains))
            {
                Console.WriteLine("Cannot add and remove the same tag at once!");
                return;
            }
            if (!remove.Any() && !add.Any())
            {
                Console.WriteLine("None of the provided tags where valid!");
                return;
            }
            // tags are now unique, order of add/remove doesn't matter anymore
            var currentTags = issue.Tags.ToList();
            foreach (var t in add)
            {
                if (!currentTags.Contains(t))
                    currentTags.Add(t);
            }
            foreach (var t in remove)
            {
                if (currentTags.Contains(t))
                    currentTags.Remove(t);
            }
            string message = "";
            if (add.Length > 0)
            {
                message += $"Added tag{(add.Length > 1 ? "(s)" : "")}: {string.Join(", ", add.ToList())}.";
            }
            if (remove.Length > 0)
            {
                if (!string.IsNullOrEmpty(message))
                    message += Environment.NewLine;

                message += $"Removed tag{(remove.Length > 1 ? "(s)" : "")}: {string.Join(", ", remove.ToList())}.";
            }
            issue.Add(new Comment(message, CurrentUser, DateTime.Now, false));

            issue.Tags = currentTags.ToArray();

            Storage.SaveIssue(issue, false);
            Console.WriteLine(message);
        }

        /// <summary>
        /// Comments on the provided issue.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        public virtual void CommentIssue(int id, string message)
        {
            var issues = Storage.LoadIssues();
            var issue = issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                Console.WriteLine($"No issue with id '#{id}' found!");
                return;
            }
            issue.Add(new Comment(message, CurrentUser, DateTime.Now, true));
            Storage.SaveIssue(issue, false);
            Console.WriteLine("Comment added!");
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
                            // don't remove open issues, that's all we want
                            return false;
                        }

                        if (s == IssueState.Closed && i.State == IssueState.Closed)
                        {
                            // don't remove closed, that's all we want
                            return false;
                        }
                        // no match; remove
                        return true;
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

        /// <summary>
        /// Changes the issue state of the specific issue where possible.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="targetState"></param>
        private void ChangeIssueState(int id, IssueState targetState)
        {
            if (targetState == IssueState.Open)
                throw new NotSupportedException("Issues can only be closed or reopenend.");

            var issues = Storage.LoadIssues();
            var issue = issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                Console.WriteLine($"No issue with id '#{id}' found!");
                return;
            }
            if (issue.State == targetState)
            {
                // nothing to do
                return;
            }

            // adding comment with changestate will change the issue state
            issue.Add(new Comment($"{targetState} the issue.", CurrentUser, DateTime.Now, false)
            {
                ChangedStateTo = targetState
            });
            Storage.SaveIssue(issue, false);
            Console.WriteLine($"Issue '#{id}' {targetState.ToString().ToLower()}!", ConsoleColor.Green);
        }

        /// <summary>
        /// Displays the provided issue and all its comments.
        /// </summary>
        /// <param name="issue"></param>
        private static void DisplayIssue(Issue issue)
        {
            // use same format as list for the header
            PrintIssueList(new List<Issue> { issue }, true);
            // breakline
            var wrap = new string('_', Console.BufferWidth);
            Console.WriteLine(wrap);

            // optional message
            if (!string.IsNullOrWhiteSpace(issue.Message))
                Console.WriteLine(issue.Message);

            // format comments in a readable fashion
            var lines = new List<string[]>();
            for (int i = 0; i < issue.Comments.Count; i++)
            {
                var c = issue.Comments[i];
                var time = c.DateTime;
                string ts;
                if (time.Date == issue.CreationDate.Date)
                {
                    // same day, just timestamp is fine
                    ts = time.ToShortTimeString();
                }
                else
                {
                    if ((time - issue.CreationDate).TotalDays > 7)
                    {
                        // anything older than a week just needs day
                        ts = time.ToShortDateString();
                    }
                    else
                    {
                        // full date needed
                        ts = time.ToString();
                    }
                }
                lines.Add(new[]
                {
                    c.Author,
                    "@ " + ts + ":",
                    c.Message
                });
            }
            var formatted = ConsoleFormatter.PadElementsInLines(lines);
            formatted = formatted.Replace(Environment.NewLine, Environment.NewLine + Environment.NewLine + wrap);
            Console.WriteLine(formatted);
        }
    }
}