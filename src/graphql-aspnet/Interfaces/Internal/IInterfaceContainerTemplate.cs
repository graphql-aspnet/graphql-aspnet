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

    /// <summary>
    /// An interface that declares a template for a given graph type
    /// manages applied interfaces.
    /// </summary>
    public interface IInterfaceContainerTemplate
    {
        /// <summary>
        /// Gets the set of interfaces that were declared on the type.
        /// </summary>
        /// <value>The declared interfaces.</value>
        IEnumerable<Type> DeclaredInterfaces { get; }
    }
}