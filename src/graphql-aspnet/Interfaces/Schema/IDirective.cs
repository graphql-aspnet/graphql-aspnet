﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An interface describing a directive in the schema type system.
    /// </summary>
    public interface IDirective : IGraphType, IGraphArgumentContainer, ISecurableSchemaItem, ITypedSchemaItem
    {
        /// <summary>
        /// Gets the resolver asssigned to this directive type to process any invocations.
        /// </summary>
        /// <value>The resolver.</value>
        IGraphDirectiveResolver Resolver { get; }

        /// <summary>
        /// Gets the bit-wise set of locations this directive can be defined at.
        /// </summary>
        /// <value>The locations.</value>
        DirectiveLocation Locations { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this directive is repeatable at a given
        /// target location.
        /// </summary>
        /// <value><c>true</c> if this directive is repeatable; otherwise, <c>false</c>.</value>
        bool IsRepeatable { get; set; }
    }
}