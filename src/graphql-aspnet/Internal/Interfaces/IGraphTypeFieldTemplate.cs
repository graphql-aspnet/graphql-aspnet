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
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.TypeTemplates;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A base template definition for the metadata about a graph field belonging to a real type (as opposed to a virutal
    /// type).
    /// </summary>
    public interface IGraphTypeFieldTemplate : IGraphFieldBaseTemplate, ISecureItem, IDeprecatable
    {
        /// <summary>
        /// Retrieves the concrete types that this instance may return or make use of in response to a field request.
        /// </summary>
        /// <returns>IEnumerable&lt;Type&gt;.</returns>
        IEnumerable<DependentType> RetrieveRequiredTypes();

        /// <summary>
        /// Creates a resolver capable of resolving this field.
        /// </summary>
        /// <returns>IGraphFieldResolver.</returns>
        IGraphFieldResolver CreateResolver();

        /// <summary>
        /// Gets the return type of this field as its declared in the C# code base with no modifications or
        /// coerions applied.
        /// </summary>
        /// <value>The type naturally returned by this field.</value>
        Type DeclaredReturnType { get; }

        /// <summary>
        /// Gets the name this field is declared as in the C# code (method name or property name).
        /// </summary>
        /// <value>The name of the declared.</value>
        string DeclaredName { get; }

        /// <summary>
        /// Gets the mode in which this type extension will be processed by the runtime.
        /// </summary>
        /// <value>The mode.</value>
        FieldResolutionMode Mode { get; }

        /// <summary>
        /// Gets the parent owner of this field.
        /// </summary>
        /// <value>The parent.</value>
        IGraphTypeTemplate Parent { get; }

        /// <summary>
        /// Gets the union proxy instance delcared as a return of this field. May be null.
        /// </summary>
        /// <value>The union proxy.</value>
        IGraphUnionProxy UnionProxy { get; }

        /// <summary>
        /// Gets the kind of graph type that should own fields created from this template.
        /// </summary>
        /// <value>The kind.</value>
        TypeKind OwnerTypeKind { get; }

        /// <summary>
        /// Gets  an estimated weight value of this field in terms of the overall impact it has on the execution of a query.
        /// See the documentation for an understanding of how query complexity is calculated.
        /// </summary>
        /// <value>The estimated complexity value for this field.</value>
        float? Complexity { get; }
    }
}