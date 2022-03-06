﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// Describes a single field in the type system. This describes how a given field is to be represented with its
    /// accepted arguments, any nullable or list modifiers and publication and depreciation information.
    /// </summary>
    public interface IGraphField : IDeprecatable, IGraphArgumentContainer, ISchemaItem
    {
        /// <summary>
        /// Updates the known graph type this field belongs to.
        /// </summary>
        /// <param name="parent">The new parent.</param>
        void AssignParent(IGraphType parent);

        /// <summary>
        /// Updates the field resolver used by this graph field.
        /// </summary>
        /// <param name="newResolver">The new resolver this field should use.</param>
        /// <param name="mode">The new resolution mode used by the runtime to invoke the resolver.
        /// When null, the current resolution mode of this field is retained.</param>
        void UpdateResolver(IGraphFieldResolver newResolver, FieldResolutionMode? mode = null);

        /// <summary>
        /// Gets or sets the type expression that represents the data returned from this field (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is a leaf field; one capable of generating
        /// a real data item vs. generating data to be used in down stream projections.
        /// </summary>
        /// <value><c>true</c> if this instance is a leaf field; otherwise, <c>false</c>.</value>
        bool IsLeaf { get; }

        /// <summary>
        /// <para>Gets an object that will perform some operation against an execution
        /// context to fulfill the requirements of this resolvable entity.
        /// </para>
        /// <remarks>
        /// Call <see cref="UpdateResolver"/> to change.
        /// </remarks>
        /// </summary>
        /// <value>The resolver assigned to this instance.</value>
        IGraphFieldResolver Resolver { get; }

        /// <summary>
        /// Gets the resolution mode of this field which instructs the query plan executor on
        /// how to process this field against its source data.
        /// </summary>
        /// <value>The mode.</value>
        FieldResolutionMode Mode { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphField" /> is published
        /// in the schema delivered to introspection requests.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        bool Publish { get; set; }

        /// <summary>
        /// Gets or sets an estimated weight value of this field in terms of the overall impact it has on the execution of a query.
        /// See the documentation for an understanding of how query complexity is calculated.
        /// </summary>
        /// <value>The estimated complexity value for this field.</value>
        float? Complexity { get; set; }

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        GraphFieldSource FieldSource { get; }

        /// <summary>
        /// Gets the security groups, a collection of policy requirements, of which each must be met,
        /// in order to access this field.
        /// </summary>
        /// <value>The security groups.</value>
        IEnumerable<AppliedSecurityPolicyGroup> SecurityGroups { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this field points to developer created code.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        bool IsVirtual { get; }

        /// <summary>
        /// Gets the core type of the object (or objects) returned by this field. If this field
        /// is meant to return a list of items, this property represents the type of item in
        /// that list.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets .NET type of the method or property that generated this field as it was declared in code.
        /// </summary>
        /// <value>The type of the declared return.</value>
        public Type DeclaredReturnType { get; }

        /// <summary>
        /// Gets the parent item that owns this field.
        /// </summary>
        /// <value>The parent.</value>
        ISchemaItem Parent { get; }
    }
}