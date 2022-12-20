// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriptionServer
{
    using System;

    /// <summary>
    /// An globally unique id representing a single client connected to this
    /// server instance. This id is schema agnostic.
    /// </summary>
    public class SubscriptionClientId
    {
        /// <summary>
        /// Gets a reference to an empty, or non-set, id value.
        /// </summary>
        /// <value>SubscriptionClientId.</value>
        public static SubscriptionClientId Empty { get; } = new SubscriptionClientId(Guid.Empty);

        /// <summary>
        /// Creates a new, unqiue client id.
        /// </summary>
        /// <returns>SubscriptionClientId.</returns>
        public static SubscriptionClientId NewClientId() => new SubscriptionClientId(Guid.NewGuid());

        private readonly Guid _idValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClientId"/> class.
        /// </summary>
        /// <param name="idValue">The identifier value.</param>
        private SubscriptionClientId(Guid idValue)
        {
            _idValue = idValue;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is SubscriptionClientId sci)
                return this.Equals(sci);

            return false;
        }

        /// <summary>
        /// Determines of this instance is identical to the provided
        /// value.
        /// </summary>
        /// <param name="otherId">The other identifier.</param>
        /// <returns><c>true</c> if the ids are identical, <c>false</c> otherwise.</returns>
        public bool Equals(SubscriptionClientId otherId)
        {
            if (object.ReferenceEquals(otherId, null))
                return false;

            return this._idValue == otherId._idValue;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return this._idValue.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return this._idValue.ToString();
        }

        /// <summary>
        /// Implements the == operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(SubscriptionClientId ls, SubscriptionClientId rs)
        {
            return ls.Equals(rs);
        }

        /// <summary>
        /// Implements the != operator.
        /// </summary>
        /// <param name="ls">The left side of the operation.</param>
        /// <param name="rs">The right side of the operation.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(SubscriptionClientId ls, SubscriptionClientId rs)
        {
            return !(ls == rs);
        }
    }
}