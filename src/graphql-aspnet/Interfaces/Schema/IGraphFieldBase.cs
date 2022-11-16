// *************************************************************
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
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A base set of items common between all field types (input, interface or object based fields).
    /// </summary>
    public interface IGraphFieldBase : ISchemaItem
    {
        /// <summary>
        /// Updates the known graph type this field belongs to.
        /// </summary>
        /// <param name="parent">The new parent.</param>
        void AssignParent(IGraphType parent);

        /// <summary>
        /// Gets the type expression that represents the data returned from this field (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }

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

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphField" /> is published
        /// in the schema delivered to introspection requests.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        bool Publish { get; set; }

        /// <summary>
        /// Gets the core type of the object (or objects) returned by this field. If this field
        /// is meant to return a list of items, this property represents the type of item in
        /// that list.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }
    }
}