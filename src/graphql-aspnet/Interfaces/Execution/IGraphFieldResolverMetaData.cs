// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A data package describing the details necessary to
    /// invoke a <see cref="IGraphFieldResolver"/> against an object instance.
    /// </summary>
    /// <remarks>
    /// This interface describes the "bridge" between a field on an schema
    /// and the C# code from which that field originated.
    /// </remarks>
    public interface IGraphFieldResolverMetaData
    {
        /// <summary>
        /// Gets the type template from which this method was generated.
        /// </summary>
        /// <value>The type template that owns this method.</value>
        IGraphTypeTemplate Parent { get; }

        /// <summary>
        /// Gets the singular concrete type this method is defined on.
        /// </summary>
        /// <value>The objec type that defines this method.</value>
        Type ObjectType { get; }

        /// <summary>
        /// Gets the type, unwrapped of any tasks, that this graph method should return upon completion. This value
        /// represents the implementation return type as opposed to the expected graph type.
        /// </summary>
        /// <value>The expected type of data returned by this method.</value>
        Type ExpectedReturnType { get; }

        /// <summary>
        /// Gets the method to be called on the target object or struct instance.
        /// </summary>
        /// <value>The method to be invoked.</value>
        MethodInfo Method { get; }

        /// <summary>
        /// Gets the raw parameters that exist on the <see cref="Method"/>
        /// that must be supplied at invocation.
        /// </summary>
        /// <value>The parameters of the target <see cref="Method"/>.</value>
        IReadOnlyList<ParameterInfo> Parameters { get; }

        /// <summary>
        /// Gets a value indicating whether the method described by this instance should be
        /// invoked asyncronously.
        /// </summary>
        /// <value><c>true</c> if the method is asynchronous; otherwise, <c>false</c>.</value>
        bool IsAsyncField { get; }

        /// <summary>
        /// Gets the method's field name in the object graph.
        /// </summary>
        /// <value>This method's name in the object graph.</value>
        string Name { get; }

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the
        /// .NET code (e.g. <c>Namespace.ObjectType.MethodName</c>).
        /// </summary>
        /// <value>The fully qualified name given to this item.</value>
        string InternalFullName { get; }

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application;
        /// typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        string InternalName { get; }

        /// <summary>
        /// Gets the unique route that points to the field in the object graph.
        /// </summary>
        /// <value>The route.</value>
        SchemaItemPath Route { get; }

        /// <summary>
        /// Gets the templatized field arguments representing the field (if any).
        /// </summary>
        /// <value>The arguments defined on this field.</value>
        IReadOnlyList<IGraphArgumentTemplate> Arguments { get; }
    }
}