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

    /// <summary>
    /// A set of metadata items for the parameters of a given resolver.
    /// </summary>
    public interface IGraphFieldResolverParameterMetaDataCollection : IReadOnlyList<IGraphFieldResolverParameterMetaData>
    {
        /// <summary>
        /// Attempts to find a parameter by its declared name in source code. This name is case sensitive.
        /// If not found, null is returned.
        /// </summary>
        /// <param name="parameterName">Name of the parameter as it exists in source code.</param>
        /// <returns>IGraphFieldResolverParameterMetaData.</returns>
        IGraphFieldResolverParameterMetaData FindByName(string parameterName);

        /// <summary>
        /// Gets the <see cref="IGraphFieldResolverParameterMetaData"/> with the specified parameter name. This
        /// name is case sensitive and should match the parameter declaration in the source code.
        /// </summary>
        /// <param name="parameterName">Name of the parameter as it exists in source code.</param>
        /// <returns>IGraphFieldResolverParameterMetaData.</returns>
        IGraphFieldResolverParameterMetaData this[string parameterName] { get; }

        /// <summary>
        /// Gets the parameter that is to be filled with the value that is supplying source data for the
        /// field that will be resolved. (e.g. the result of the nearest ancestor in the query.
        /// </summary>
        /// <value>The source parameter metadata item.</value>
        IGraphFieldResolverParameterMetaData SourceParameter { get; }
    }
}