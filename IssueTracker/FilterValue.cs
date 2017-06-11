namespace IssueTracker
{
    /// <summary>
    /// Container to allow issue filtering.
    /// </summary>
    public class FilterValue
    {
        /// <summary>
        /// The type of the filter.
        /// </summary>
        public Filter FilterType { get; set; }

        /// <summary>
        /// The filter value. Depends on the type.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="v"></param>
        public FilterValue(Filter f, object v)
        {
            FilterType = f;
            Value = v;
        }
    }
}