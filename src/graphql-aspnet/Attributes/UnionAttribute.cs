// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An attribute that, when applied to an action method, field or property
    /// declares that the field is to return one of multiple possible types.
    /// </summary>
    /// <remarks>
    /// Fields applying this attribute should return a <see cref="IGraphActionResult"/> or <see cref="object"/>
    /// for maximum compatiability.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class UnionAttribute : GraphAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnionAttribute" /> class.
        /// </summary>
        /// <param name="unionName">The name of the union as it should appear in the schema.</param>
        /// <param name="firstUnionMemberType">The first member type to include in the union.</param>
        /// <param name="otherUnionMembers">Additional member types to include in the union.</param>
        /// <remarks>All Unions must declare at least two member types that will be included in the union.</remarks>
        public UnionAttribute(string unionName, Type firstUnionMemberType, params Type[] otherUnionMembers)
        {
            this.UnionName = unionName?.Trim();

            var list = new List<Type>(2 + otherUnionMembers.Length);
            list.Add(firstUnionMemberType);
            list.AddRange(otherUnionMembers);
            this.UnionMemberTypes = list;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionAttribute"/> class.
        /// </summary>
        /// <param name="unionProxyType">A type that inherits from <see cref="GraphUnionProxy"/> or implements <see cref="IGraphUnionProxy"/> which
        /// declares all required information about the referenced union.</param>
        public UnionAttribute(Type unionProxyType)
            : this()
        {
            this.UnionName = null;
            if (unionProxyType != null)
                this.UnionMemberTypes.Add(unionProxyType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionAttribute" /> class.
        /// </summary>
        /// <param name="unionName">The name to assign to this union.</param>
        public UnionAttribute(string unionName)
            : this()
        {
            this.UnionName = unionName;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="UnionAttribute"/> class from being created.
        /// </summary>
        private UnionAttribute()
        {
            this.UnionMemberTypes = new List<Type>();
        }

        /// <summary>
        /// Gets or sets the name of the new union to create within the schema. This union name must be a valid graph name.
        /// </summary>
        /// <value>The name of the union as it will appear in the schema.</value>
        public string UnionName { get; set; }

        /// <summary>
        /// Gets or sets the concrete types of the objects that may be returned by this field.
        /// </summary>
        /// <value>All union member types to be included in this union.</value>
        public IList<Type> UnionMemberTypes { get; set; }
    }
}