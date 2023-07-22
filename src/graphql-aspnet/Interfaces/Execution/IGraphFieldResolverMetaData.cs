﻿// *************************************************************
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
    using System.Reflection;

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
        /// Gets the type, unwrapped of any tasks, that this graph method should return upon completion. This value
        /// represents the implementation return type that is needed by the runtime to be success. It may differ from the
        /// declared return type of <see cref="Method"/> in the case of returning interfaces.
        /// </summary>
        /// <value>The expected type of data returned by this method.</value>
        Type ExpectedReturnType { get; }

        /// <summary>
        /// Gets the method to be called on the target object or struct instance.
        /// </summary>
        /// <value>The method to be invoked.</value>
        MethodInfo Method { get; }

        /// <summary>
        /// Gets a value indicating whether the method described by this instance should be
        /// invoked asyncronously.
        /// </summary>
        /// <value><c>true</c> if the method is asynchronous; otherwise, <c>false</c>.</value>
        bool IsAsyncField { get; }

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application;
        /// typically a qualifed method name or property name. (e.g. <c>MyObject.MyMethodName</c>).
        /// </summary>
        /// <value>The internal name given to this item.</value>
        string InternalName { get; }

        /// <summary>
        /// Gets the type representing the graph type that will invoke the resolver identified by this
        /// metadata object.
        /// </summary>
        /// <value>The concrete type of the parent object that owns the resolver.</value>
        Type ParentObjectType { get; }

        /// <summary>
        /// Gets the internal name of the parent item that ows the <see cref="Method"/> which generated
        /// this metdata object.
        /// </summary>
        /// <value>The name of the parent.</value>
        string ParentInternalName { get; }

        /// <summary>
        /// Gets the templatized field arguments representing the field (if any).
        /// </summary>
        /// <value>The arguments defined on this field.</value>
        IGraphFieldResolverParameterMetaDataCollection Parameters { get; }
    }
}