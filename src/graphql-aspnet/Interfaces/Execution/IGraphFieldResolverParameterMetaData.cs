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
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A metadata object containing parsed and computed values related to a single parameter
    /// on a C# method that is used a a resolver to a graph field.
    /// </summary>
    /// <remarks>
    /// This metadata object is expressed in terms of the implementation method. That is, .NET <see cref="ParameterInfo"/>
    /// terms not GraphQL <see cref="IGraphArgument"/> terms.
    /// </remarks>
    public interface IGraphFieldResolverParameterMetaData
    {
        /// <summary>
        /// Gets the parameter info that defines the argument.
        /// </summary>
        /// <value>The method to be invoked.</value>
        ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Gets the type of data expected to be passed to this parameter when its target
        /// method is invoked.
        /// </summary>
        /// <value>The expected type of data this parameter can accept.</value>
        Type ExpectedType { get; }

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
        /// Gets the set of argument modifiers created for this parameter via the template that created
        /// it.
        /// </summary>
        /// <value>The argument modifiers.</value>
        GraphArgumentModifiers ArgumentModifiers { get; }

        /// <summary>
        /// Gets the default value assigned to this parameter, if any.
        /// </summary>
        /// <value>The default value.</value>
        object DefaultValue { get; }
    }
}