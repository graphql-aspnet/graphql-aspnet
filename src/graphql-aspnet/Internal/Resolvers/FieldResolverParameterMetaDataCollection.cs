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
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of parameter metadata that can be accessed by index in its parent method or by name.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
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

            foreach (var paramItem in _parameters)
            {
                _parametersByName.Add(paramItem.ParameterInfo.Name, paramItem);
                if (paramItem.ArgumentModifiers.IsSourceParameter())
                    this.SourceParameter = paramItem;
            }
        }

        /// <inheritdoc />
        public IGraphFieldResolverParameterMetaData FindByName(string parameterName)
        {
            if (parameterName == null)
                return null;

            if (_parametersByName.ContainsKey(parameterName))
                return _parametersByName[parameterName];

            return null;
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

        /// <inheritdoc />
        public IGraphFieldResolverParameterMetaData SourceParameter { get; }
    }
}