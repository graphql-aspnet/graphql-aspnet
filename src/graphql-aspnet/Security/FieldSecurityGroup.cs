// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Security
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// A group of <see cref="FieldSecurityPolicy"/>s describing the collected security requirements
    /// of an entity such as the policies applied to a controller or to a field method.
    /// </summary>
    public sealed class FieldSecurityGroup : IEnumerable<FieldSecurityPolicy>
    {
        /// <summary>
        /// Gets a static instance of an empty set of requirements. This instance allows anonymous access.
        /// </summary>
        /// <value>The empty.</value>
        public static FieldSecurityGroup Empty { get; }

        /// <summary>
        /// Generates a security group from a given attribute container.
        /// </summary>
        /// <param name="attributedItem">The attributed item to build the group from.</param>
        /// <returns>SecurityGroup.</returns>
        public static FieldSecurityGroup FromAttributeCollection(ICustomAttributeProvider attributedItem)
        {
            var group = new FieldSecurityGroup
            {
                _requirements = attributedItem.GetCustomAttributes(true)
                        .Where(x => x is Attribute && x is IAuthorizeData)
                        .OfType<IAuthorizeData>()
                        .Select(x => new FieldSecurityPolicy(x)).ToList(),

                AllowAnonymous = attributedItem.GetCustomAttributes(true)
                        .Any(x => x is Attribute && x is IAllowAnonymous),
            };

            return group;
        }

        static FieldSecurityGroup()
        {
            Empty = new FieldSecurityGroup
            {
                AllowAnonymous = true,
            };
        }

        private List<FieldSecurityPolicy> _requirements;

        /// <summary>
        /// Prevents a default instance of the <see cref="FieldSecurityGroup"/> class from being created.
        /// </summary>
        private FieldSecurityGroup()
        {
            _requirements = new List<FieldSecurityPolicy>();
        }

        /// <summary>
        /// Gets a count of the number of requirements in this group.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _requirements.Count;

        /// <summary>
        /// Gets a value indicating whether this group permits anonymous access or not.
        /// </summary>
        /// <value><c>true</c> if [allow anonymous]; otherwise, <c>false</c>.</value>
        public bool AllowAnonymous { get; private set; }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<FieldSecurityPolicy> GetEnumerator()
        {
            return _requirements.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}