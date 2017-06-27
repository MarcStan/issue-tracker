using MadMilkman.Ini;
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
        private const string RootFileName = ".issues";

        private readonly Storage _storage;

        /// <summary>
        /// Creates a new instance that works in the provided directory.
        /// </summary>
        /// <param name="workingDirectory"></param>
        public IssueTracker(string workingDirectory)
        {
            if (string.IsNullOrEmpty(workingDirectory))
                throw new ArgumentNullException(nameof(workingDirectory));

            _workingDirectory = Path.GetFullPath(workingDirectory);
            _storage = new Storage(_workingDirectory);
        }

        /// <summary>
        /// Gets the fully qualified path for the root file.
        /// </summary>
        public string RootFile => Path.Combine(_workingDirectory, RootFileName);

        /// <summary>
        /// Returns the current user name as found in the .gitconfig.
        /// </summary>
        public virtual string CurrentUser => GitHelper.GetUserName();

        /// <summary>
        /// Gets whether the working directory is an issue tracker.
        /// </summary>
        public virtual bool WorkingDirectoryIsIssueTracker => File.Exists(RootFile);

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
            File.WriteAllText(RootFile, $"owner={user}{Environment.NewLine}titleTrim=50");

            Console.WriteLine($"New issue project created for owner {user}!");
        }

        /// <summary>
        /// Lists the issues that match the provided set of filters.
        /// </summary>
        /// <param name="filters"></param>
        public virtual void ListIssues(List<FilterValue> filters)
        {
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            AssertIssueTracker();

            // forbid duplicate filters, as they are pointless. E.g. 2x filter user will always return 0 results when there are 2 different names
            // same for issue state
            if (filters.Select(f => f.FilterType).Distinct().Count() != filters.Count)
                throw new NotSupportedException("Each filter type may only be used once!");

            var issues = _storage.LoadIssues();
            foreach (var f in filters)
            {
                ApplyFilter(f, issues);
            }
            if (!issues.Any())
            {
                Console.WriteLine("Found no matching issues!");
                return;
            }
            Console.WriteLine($"Found {issues.Count} matching issues:");
            Console.WriteLine();
            PrintIssueList(issues, TitleLimit);
        }

        /// <summary>
        /// Returns the title trim value from the .issues file.
        /// </summary>
        public int TitleLimit
        {
            get
            {
                try
                {
                    using (var reader = File.OpenRead(RootFile))
                    using (var stream = new MemoryStream())
                    {
                        reader.CopyTo(stream);
                        stream.Seek(0, SeekOrigin.Begin);
                        var ini = new IniFile();
                        ini.Load(stream);

                        var value = ini.Sections.First().Keys["titleTrim"]?.Value;
                        return int.Parse(value);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failure while looking up titleTrim in {RootFileName} file. " + e.Message);
                    Console.WriteLine();
                    return 50;
                }
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
            AssertIssueTracker();

            ChangeIssueState(id, IssueState.Open);
        }

        /// <summary>
        /// Reopens the isse with the provided id (if found and open).
        /// </summary>
        /// <param name="id"></param>
        public virtual void CloseIssue(int id)
        {
            AssertIssueTracker();

            ChangeIssueState(id, IssueState.Closed);
        }

        /// <summary>
        /// Displays the provided issue with all its comments.
        /// </summary>
        /// <param name="id"></param>
        public virtual void ShowIssue(int id)
        {
            AssertIssueTracker();

            var issues = _storage.LoadIssues();
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

            var all = _storage.LoadIssues();
            var maxId = all.Any() ? all.Max(i => i.Id) : 0;
            var issue = new Issue(maxId + 1, title, message, tags, DateTime.Now, CurrentUser, null, IssueState.Open, -1);
            _storage.SaveIssue(issue, true);
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
            if (add == null || remove == null)
                throw new ArgumentNullException();

            AssertIssueTracker();

            var issues = _storage.LoadIssues();
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
            var addedTags = new List<Tag>();
            var removedTags = new List<Tag>();
            foreach (var t in add)
            {
                if (!currentTags.Contains(t))
                {
                    currentTags.Add(t);
                    addedTags.Add(t);
                }
                else
                {
                    Console.WriteLine($"Tag '{t}' already exists!");
                }
            }
            foreach (var t in remove)
            {
                if (currentTags.Contains(t))
                {
                    currentTags.Remove(t);
                    removedTags.Add(t);
                }
                else
                {
                    Console.WriteLine($"Tag '{t}' does not exist!");
                }
            }
            string message = "";
            if (addedTags.Count > 0)
            {
                message += $"Added tag{(addedTags.Count > 1 ? "(s)" : "")}: {string.Join(", ", addedTags)}.";
            }
            if (removedTags.Count > 0)
            {
                if (!string.IsNullOrEmpty(message))
                    message += Environment.NewLine;

                message += $"Removed tag{(addedTags.Count > 1 ? "(s)" : "")}: {string.Join(", ", removedTags)}.";
            }
            if (addedTags.Any() || removedTags.Any())
            {
                issue.Add(new Comment(message, CurrentUser, DateTime.Now, false));

                issue.Tags = currentTags.ToArray();

                _storage.SaveIssue(issue, false);
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine("No changes where made!");
            }
        }

        /// <summary>
        /// Comments on the provided issue.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        public virtual void CommentIssue(int id, string message)
        {
            AssertIssueTracker();

            var issues = _storage.LoadIssues();
            var issue = issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                Console.WriteLine($"No issue with id '#{id}' found!");
                return;
            }
            issue.Add(new Comment(message, CurrentUser, DateTime.Now, true));
            _storage.SaveIssue(issue, false);
            Console.WriteLine("Comment added!");
        }

        /// <summary>
        /// Prints the list of issues in a human readable fashion.
        /// </summary>
        /// <param name="issues"></param>
        /// <param name="titleLimit">Optional. Allows title to be trimmed to a length</param>
        private static void PrintIssueList(List<Issue> issues, int? titleLimit = null)
        {
            if (titleLimit.HasValue && titleLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(titleLimit));

            var formatted = new List<string[]>();
            foreach (var i in issues)
            {
                var tagList = i.Tags.Length > 0 ? "[" + string.Join(", ", i.Tags.Select(t => t.Name)) + "]" : "";

                var cc = i.Comments.Count(c => c.Editable);
                var comments = cc > 0 ? $" ({cc} comments)" : "";
                var title = i.Title;
                if (titleLimit.HasValue && title.Length > titleLimit.Value)
                {
                    title = title.Substring(0, titleLimit.Value) + "..";
                }
                formatted.Add(new[]
                {
                    $"#{i.Id}",
                    $"[{i.State}]",
                    $"'{title}'",
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
                    issues.RemoveAll(i =>
                    {
                        foreach (var t in tags)
                        {
                            if (!i.Tags.Contains(t))
                                return true;
                        }
                        return false;
                    });
                    break;
                case Filter.IssueState:
                    var s = (IssueState)filter.Value;
                    issues.RemoveAll(i =>
                    {
                        if (s == IssueState.Open && i.State == IssueState.Open)
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
                    issues.RemoveAll(i => !i.Author.Equals(user, StringComparison.InvariantCultureIgnoreCase));
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
            var issues = _storage.LoadIssues();
            var issue = issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                Console.WriteLine($"No issue with id '#{id}' found!");
                return;
            }
            if (issue.State == targetState)
            {
                // nothing to do
                Console.WriteLine("Already the case!");
                return;
            }


            var localizedState = targetState == IssueState.Open ? "Reopenend" : "Closed";
            // adding comment with changestate will change the issue state
            issue.Add(new Comment($"{localizedState} the issue.", CurrentUser, DateTime.Now, false)
            {
                ChangedStateTo = targetState
            });
            _storage.SaveIssue(issue, false);
            Console.WriteLine($"Issue '#{id}' {targetState.ToString().ToLower()}!");
        }

        /// <summary>
        /// Displays the provided issue and all its comments.
        /// </summary>
        /// <param name="issue"></param>
        private static void DisplayIssue(Issue issue)
        {

            int width;
            if (!ConsoleEx.GetConsoleWindowWidth(out width))
            {
                //only the case for unit tests, due to no attached console
                // but return sane default anyway
                width = 80;
            }

            var hwrap = new string('=', width);
            // header
            var title = $" ISSUE #{issue.Id} ";
            int start = (width - title.Length) / 2;
            title = new string('=', start) + title;
            Console.Write(hwrap);
            Console.Write(title + new string('=', width - title.Length));
            Console.WriteLine(hwrap);
            // use same format as list for the header
            PrintIssueList(new List<Issue> { issue });
            // breakline
            var wrap = new string('_', width);
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