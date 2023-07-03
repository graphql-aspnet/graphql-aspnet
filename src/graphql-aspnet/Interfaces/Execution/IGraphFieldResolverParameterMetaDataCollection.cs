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
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A set of metadata items for the parameters of a given resolver.
    /// </summary>
    public interface IGraphFieldResolverParameterMetaDataCollection : IReadOnlyList<IGraphFieldResolverParameterMetaData>
    {
        /// <summary>
        /// Gets the <see cref="IGraphFieldResolverParameterMetaData"/> with the specified parameter name. This
        /// name is case sensitive and should match the parameter declaration in the source code.
        /// </summary>
        /// <param name="parameterName">Name of the parameter as it exists in source code.</param>
        /// <returns>IGraphFieldResolverParameterMetaData.</returns>
        IGraphFieldResolverParameterMetaData this[string parameterName] { get; }
    }
}