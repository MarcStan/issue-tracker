using System;

namespace IssueTracker
{
    /// <summary>
    /// Container for comments added to issues.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="author"></param>
        /// <param name="dateTime"></param>
        /// <param name="editable"></param>
        public Comment(string message, string author, DateTime dateTime, bool editable)
        {
            Message = message;
            Author = author;
            DateTime = dateTime;
            Editable = editable;
        }

        /// <summary>
        /// The message of the comment.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The author of the comment.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// The post date of the comment.
        /// </summary>
        public DateTime DateTime { get; }

        /// <summary>
        /// Whether the comment is editable.
        /// This defaults to true for all user comments and false for all system comments.
        /// </summary>
        public bool Editable { get; }

        /// <summary>
        /// The new issue state this comment applies.
        /// This is only used by system comments and requires <see cref="Editable"/> to be false.
        /// </summary>
        public IssueState? ChangedStateTo { get; internal set; }
    }
}