using System;
using System.Collections.Generic;

namespace IssueTracker.Tests
{
    /// <summary>
    /// A wrapper around the actual issue tracker.
    /// This will intercept all method calls and invoke the event handlers in their place.
    /// </summary>
    public class MockIssueTracker : IssueTracker
    {
        /// <summary>
        /// Central event that is fired by each method as it is being called.
        /// The command name will be the long argument name that the user would use for each method (init, add, edit, ...).
        /// </summary>
        public event EventHandler<IssueTrackerCommandArgs> OnMethodCalled;

        public MockIssueTracker(string workingDirectory) : base(workingDirectory)
        {
        }

        public override string CurrentUser => "NUnit";

        public override void AddIssue(string title, string message, Tag[] tags)
        {
            OnMethodCalled?.Invoke(this, new IssueTrackerCommandArgs("add", new Dictionary<string, object>
            {
                {"title", title},
                {"message", message},
                {"tags", tags}
            }));
        }

        public override void CloseIssue(int id)
        {
            OnMethodCalled?.Invoke(this, new IssueTrackerCommandArgs("close", new Dictionary<string, object>
            {
                {"id", id }
            }));
        }

        public override void CommentIssue(int id, string message)
        {
            OnMethodCalled?.Invoke(this, new IssueTrackerCommandArgs("comment", new Dictionary<string, object>
            {
                {"id", id }
            }));
        }

        public override void EditTags(int id, Tag[] add, Tag[] remove)
        {
            OnMethodCalled?.Invoke(this, new IssueTrackerCommandArgs("edit", new Dictionary<string, object>
            {
                {"id", id },
                {"add", add },
                {"remove", remove}
            }));
        }

        public override void InitializeNewProject()
        {
            OnMethodCalled?.Invoke(this, new IssueTrackerCommandArgs("init", null));
        }

        public override void ListIssues(List<FilterValue> filters)
        {
            OnMethodCalled?.Invoke(this, new IssueTrackerCommandArgs("list", new Dictionary<string, object>
            {
                {"filters", filters }
            }));
        }

        public override void ReopenIssue(int id)
        {
            OnMethodCalled?.Invoke(this, new IssueTrackerCommandArgs("reopen", new Dictionary<string, object>
            {
                {"id", id }
            }));
        }

        public override void ShowIssue(int id)
        {
            OnMethodCalled?.Invoke(this, new IssueTrackerCommandArgs("show", new Dictionary<string, object>
            {
                {"id", id }
            }));
        }
    }

    /// <summary>
    /// Argument for the mock issue tracker event.
    /// </summary>
    public class IssueTrackerCommandArgs : EventArgs
    {
        /// <summary>
        /// The additional arguments (depend on the specific command).
        /// </summary>
        public Dictionary<string, object> Args { get; }

        /// <summary>
        /// The command that causes this event to be raised.
        /// </summary>
        public string Command { get; }

        public IssueTrackerCommandArgs(string command, Dictionary<string, object> args)
        {
            Args = args ?? new Dictionary<string, object>();
            Command = command;
        }
    }
}