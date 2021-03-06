using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace IssueTracker
{
    /// <summary>
    /// Container for a single issue.
    /// </summary>
    [DebuggerDisplay("{Id} {Title}")]
    public class Issue
    {
        private readonly List<Comment> _comments = new List<Comment>();

        /// <summary>
        /// Creates a new issue with the provided values.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="tags"></param>
        /// <param name="creationDate"></param>
        /// <param name="author"></param>
        public Issue(int id, string title, string message, Tag[] tags, DateTime creationDate, string author) : this(id, title, message, tags, creationDate, author, null, IssueState.Open, -1)
        {
        }

        /// <summary>
        /// Internal.
        /// Used to load issues from disk.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="tags"></param>
        /// <param name="creationDate"></param>
        /// <param name="author"></param>
        /// <param name="comments"></param>
        /// <param name="state"></param>
        /// <param name="lastStateChangeCommentIndex"></param>
        internal Issue(int id, string title, string message, Tag[] tags, DateTime creationDate, string author, List<Comment> comments, IssueState state, int lastStateChangeCommentIndex)
        {
            Id = id;
            Title = title;
            Message = message;
            Tags = tags ?? new Tag[0];
            CreationDate = creationDate;
            Author = author;
            _comments = comments ?? new List<Comment>();
            State = state;
            LastStateChangeCommentIndex = lastStateChangeCommentIndex;
        }

        /// <summary>
        /// The unique id.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// The user provided title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The user provided message for this issue (optional).
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The set of tags provided by the user (if any).
        /// Is never null but may be empty.
        /// </summary>
        public Tag[] Tags { get; internal set; }

        /// <summary>
        /// The timestamp when the issue was created.
        /// </summary>
        public DateTime CreationDate { get; }

        /// <summary>
        /// The name of the author.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// A list of comments that have been added to this issue.
        /// Is never null but may be empty.
        /// </summary>
        public IReadOnlyList<Comment> Comments => _comments;

        /// <summary>
        /// The current state of the issue.
        /// Indicates whether further changes can be made.
        /// </summary>
        public IssueState State { get; private set; }

        /// <summary>
        /// Index of the last comment that is related to the state change.
        /// </summary>
        public int LastStateChangeCommentIndex { get; private set; }

        /// <summary>
        /// Adds the specific comment to the current issue.
        /// </summary>
        /// <param name="c"></param>
        public void Add(Comment c)
        {
            if (c == null)
                throw new ArgumentNullException(nameof(c));

            if (c.ChangedStateTo.HasValue)
            {
                State = c.ChangedStateTo.Value;
                LastStateChangeCommentIndex = _comments.Count;
            }
            _comments.Add(c);
        }
    }
}