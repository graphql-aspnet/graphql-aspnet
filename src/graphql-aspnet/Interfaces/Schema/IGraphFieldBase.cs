// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    using System;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A base set of items common between all field types (input, interface or object based fields).
    /// </summary>
    public interface IGraphFieldBase : ITypedSchemaItem, ISchemaItem
    {
        /// <summary>
        /// Gets the type expression that represents the data returned from this field (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets .NET return type of the method or property that generated this field as it was declared in code. This
        /// type may include task wrappers etc.
        /// </summary>
        /// <value>The .NET declared type returned from this field.</value>
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
    }
}