using System;

namespace IssueTracker
{
    /// <summary>
    /// A tag is a label applied to an issue to group it with other, similar issues.
    /// </summary>
    public class Tag : IEquatable<Tag>
    {
        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="name"></param>
        public Tag(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            if (name.Contains(","))
                throw new ArgumentException("Tags may not contain ','");

            Name = name;
        }

        /// <summary>
        /// The tag name.
        /// </summary>
        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Tag other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Tag)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(Tag l, Tag r)
        {
            if (ReferenceEquals(l, null))
                return ReferenceEquals(r, null);
            return l.Equals(r);
        }

        public static bool operator !=(Tag l, Tag r)
        {
            if (ReferenceEquals(l, null))
                return !ReferenceEquals(r, null);
            return !l.Equals(r);
        }
    }
}