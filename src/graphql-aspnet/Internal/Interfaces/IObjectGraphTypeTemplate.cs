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
    /// An interface describing qualified Graph object.
    /// </summary>
    public interface IObjectGraphTypeTemplate : IGraphTypeTemplate, IGraphTypeFieldTemplateContainer
    {
        /// <summary>
        /// Gets the set of interfaces that were declared on the type.
        /// </summary>
        /// <value>The declared interfaces.</value>
        IEnumerable<Type> DeclaredInterfaces { get; }
    }
}