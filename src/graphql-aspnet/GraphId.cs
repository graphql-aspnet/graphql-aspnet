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

    /// <summary>
    /// <para>A scalar representation of the graphql defined "ID" scalar.</para>
    /// <para>spec: https://graphql.github.io/graphql-spec/June2018/#sec-ID .</para>
    /// </summary>
    public struct GraphId : IEquatable<GraphId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphId" /> struct.
        /// </summary>
        /// <param name="id">The identifier to copy from.</param>
        public GraphId(GraphId id)
        {
            this.Value = id.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphId"/> struct.
        /// </summary>
        /// <param name="idValue">The identifier value.</param>
        public GraphId(string idValue)
        {
            this.Value = idValue;
        }

        /// <summary>
        /// Gets the value.
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
            return string.Equals(this.Value, other.Value);
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
                return this.Equals((GraphId)str);

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this.Value != null ? this.Value.GetHashCode() : 0;
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
            if (rs is null)
                return false;

            if (rs is string rss)
                return ls.Value == rss;

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
            if (ls is null)
                return false;

            if (ls is string lss)
                return lss == rs.Value;

            return false;
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