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
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A packaged data set pointing to a specific method on a <see cref="GraphController"/> or general <see cref="Type"/> that is to be invoked
    /// as an action to resolve a field request.
    /// </summary>
    public interface IGraphMethod
    {
        /// <summary>
        /// Gets the type that contains this method.
        /// </summary>
        /// <value>The type of the controller.</value>
        IGraphTypeTemplate Parent { get; }

        /// <summary>
        /// Gets the singular concrete type this definition represents.
        /// </summary>
        /// <value>The type of the object.</value>
        Type ObjectType { get; }

        /// <summary>
        /// Gets the action method to be called on the controller.
        /// </summary>
        /// <value>The action method.</value>
        MethodInfo Method { get; }

        /// <summary>
        /// Gets a value indicating whether the method described by this instance should be.
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
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        string InternalFullName { get; }

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        string InternalName { get; }

        /// <summary>
        /// Gets the qualified route that points to this method in the object graph.
        /// </summary>
        /// <value>The route.</value>
        GraphFieldPath Route { get; }

        /// <summary>
        /// Gets the arguments defined on the method this instance represents.
        /// </summary>
        /// <value>The arguments.</value>
        IReadOnlyList<IGraphFieldArgumentTemplate> Arguments { get; }
    }
}