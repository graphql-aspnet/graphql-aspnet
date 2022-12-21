﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using System;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An interface describing a template that can accurately represent an input argument in the object graph.
    /// </summary>
    public interface IGraphArgumentTemplate : ISchemaItemTemplate
    {
        /// <summary>
        /// Gets the parent method this parameter belongs to.
        /// </summary>
        /// <value>The parent.</value>
        IGraphFieldTemplateBase Parent { get; }

        /// <summary>
        /// Gets the default value assigned to this parameter as part of its declaration, if any.
        /// </summary>
        /// <value>The default value.</value>
        object DefaultValue { get; }

        /// <summary>
        /// Gets the type expression that represents how this field is represented in the object graph.
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets a value indicating that this argument represents the resolved data item created
        /// by the resolution of the parent field to this field. If true, this argument will not be available
        /// on the object graph.
        /// </summary>
        /// <value><c>true</c> if this instance is source data field; otherwise, <c>false</c>.</value>
        GraphArgumentModifiers ArgumentModifiers { get; }

        /// <summary>
        /// Gets the name of the argument as its declared in the server side code.
        /// </summary>
        /// <value>The name of the declared argument.</value>
        string DeclaredArgumentName { get; }

        /// <summary>
        /// Gets the input type of this argument as its declared in the C# code base with no modifications or
        /// coerions applied.
        /// </summary>
        /// <value>The type of this argument as declared in the code base.</value>
        Type DeclaredArgumentType { get; }

        /// <summary>
        /// Gets the list type wrappers, if defined, used to generate a type expression for this field.
        /// This list represents the type requirements of the field. When null or not supplied, the type
        /// expression will be inferred by the return type of the field.
        /// </summary>
        /// <value>The custom wrappers.</value>
        MetaGraphTypes[] DeclaredTypeWrappers { get; }

        /// <summary>
        /// Gets a value indicating whether this argument has a default value assigned to it.
        /// </summary>
        /// <value><c>true</c> if this instance has a default value; otherwise, <c>false</c>.</value>
        bool HasDefaultValue { get; }
    }
}