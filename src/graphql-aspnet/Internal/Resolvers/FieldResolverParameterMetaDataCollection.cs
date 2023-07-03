// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Resolvers
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A collection of parameter metadata that can be accessed by index in its parent method or by name.
    /// </summary>
    internal class FieldResolverParameterMetaDataCollection : IGraphFieldResolverParameterMetaDataCollection
    {
        private List<IGraphFieldResolverParameterMetaData> _parameters;
        private Dictionary<string, IGraphFieldResolverParameterMetaData> _parametersByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldResolverParameterMetaDataCollection" /> class.
        /// </summary>
        /// <param name="parameters">The parameters to include in this collection.</param>
        public FieldResolverParameterMetaDataCollection(IEnumerable<IGraphFieldResolverParameterMetaData> parameters = null)
        {
            parameters = parameters ?? Enumerable.Empty<IGraphFieldResolverParameterMetaData>();

            _parameters = new List<IGraphFieldResolverParameterMetaData>(parameters);

            _parametersByName = new Dictionary<string, IGraphFieldResolverParameterMetaData>(_parameters.Count);

            foreach (var arg in _parameters)
                _parametersByName.Add(arg.ParameterInfo.Name, arg);
        }

        /// <inheritdoc />
        public IEnumerator<IGraphFieldResolverParameterMetaData> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc />
        public IGraphFieldResolverParameterMetaData this[string parameterName] => _parametersByName[parameterName];

        /// <inheritdoc />
        public IGraphFieldResolverParameterMetaData this[int index] => _parameters[index];

        /// <inheritdoc />
        public int Count => _parameters.Count;
    }
}