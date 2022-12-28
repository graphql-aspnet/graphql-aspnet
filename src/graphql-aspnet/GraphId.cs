// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;

    /// <summary>
    /// <para>A scalar representation of the graphql defined "ID" scalar.</para>
    /// <para>spec: <see href="https://graphql.github.io/graphql-spec/October2021/#sec-ID" /> .</para>
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public sealed class GraphId : IEquatable<GraphId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphId" /> class.
        /// </summary>
        /// <param name="id">The graphid to copy from.</param>
        public GraphId(GraphId id)
        {
            Validation.ThrowIfNull(id, nameof(id));
            this.Value = id.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphId"/> class.
        /// </summary>
        /// <param name="idValue">The id value to wrap.</param>
        public GraphId(string idValue)
        {
            this.Value = idValue;
        }

        /// <summary>
        /// Gets the value of the Id.
        /// </summary>
        /// <value>The value.</value>
        public string Value { get; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(GraphId other)
        {
            // this non-null id can't be equal to a null'd id
            if (other == null)
                return false;

            return string.Equals(this.Value, other.Value, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (obj is GraphId id)
                return this.Equals(id);

            if (obj is string str)
                return string.Equals(this.Value, str, StringComparison.Ordinal);

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this.Value;
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GraphId ls, GraphId rs)
        {
            if (ReferenceEquals(null, ls))
                return ReferenceEquals(null, rs);

            if (ReferenceEquals(null, rs))
                return false;

            return ls.Equals(rs);
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GraphId ls, GraphId rs)
        {
            return !(ls == rs);
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(GraphId ls, object rs)
        {
            // a graph id (null or not) is never equal to another object
            // unless that other object is a string that can be cast correctly
            if (ReferenceEquals(null, ls) || ReferenceEquals(null, rs))
                return false;

            if (rs is string rss)
                return string.Equals(ls.Value, rss, StringComparison.Ordinal);

            return false;
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(GraphId ls, object rs)
        {
            return !(ls == rs);
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(object ls, GraphId rs)
        {
            return rs == ls;
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(object ls, GraphId rs)
        {
            return !(ls == rs);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="GraphId"/> to <see cref="string"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator string(GraphId id) => id.Value;

        /// <summary>
        /// Performs an explicit conversion from <see cref="string"/> to <see cref="GraphId"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The result of the conversion.</returns>
        public static explicit operator GraphId(string value) => new GraphId(value);
    }
}