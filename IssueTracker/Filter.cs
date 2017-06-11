namespace IssueTracker
{
    /// <summary>
    /// Filter for issues.
    /// </summary>
    public enum Filter
    {
        /// <summary>
        /// Filter based on tag.
        /// The filter value will be a Tag[] with at least one element.
        /// </summary>
        Tag,
        /// <summary>
        /// Filter based on the issue state.
        /// The filter value will be an issue state.
        /// </summary> 
        IssueState,
        /// <summary>
        /// Filters for the specific user (issue creator).
        /// The filter value will be a user name.
        /// </summary>
        User
    }
}