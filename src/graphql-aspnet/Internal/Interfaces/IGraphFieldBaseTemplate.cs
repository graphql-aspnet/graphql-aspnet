// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.Interfaces
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// An interface defining a field that may have some source data supplied to it to perform its
    /// resolution. (e.g. A property or method field of a model object).
    /// </summary>
    public interface IGraphFieldBaseTemplate : IGraphItemTemplate, IGraphTypeExpressionDeclaration
    {
        /// <summary>
        /// Gets a list of arguments this field can accept.
        /// </summary>
        /// <value>The parameters.</value>
        IReadOnlyList<IGraphFieldArgumentTemplate> Arguments { get; }

        /// <summary>
        /// Gets the type of the object that supplies data to this field during a resolution. This is usually
        /// the concrete type of the<see cref="IGraphItemTemplate" /> that defines this field.
        /// </summary>
        /// <value>The type of the source object.</value>
        Type SourceObjectType { get; }

        /// <summary>
        /// Gets the type expression that represents the data returned from this field (i.e. the '[SomeType!]'
        /// declaration used in schema definition language.)
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }

        /// <summary>
        /// Gets the source type this field was created from.
        /// </summary>
        /// <value>The field souce.</value>
        GraphFieldTemplateSource FieldSource { get; }
    }
}