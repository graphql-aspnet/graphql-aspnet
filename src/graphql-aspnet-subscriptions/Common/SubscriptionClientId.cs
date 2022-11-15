// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common
{
    using System;

    public class SubscriptionClientId
    {
        public static SubscriptionClientId NewClientId() => new SubscriptionClientId(Guid.NewGuid());

        private readonly Guid _idValue;

        private SubscriptionClientId(Guid idValue)
        {
            _idValue = idValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is SubscriptionClientId sci)
                return this.Equals(sci);

            return false;
        }

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