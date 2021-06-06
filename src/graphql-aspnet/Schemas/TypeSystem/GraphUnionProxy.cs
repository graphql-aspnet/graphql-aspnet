// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <inheritdoc cref="IGraphUnionProxy" />
    public class GraphUnionProxy : IGraphUnionProxy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUnionProxy"/> class.
        /// </summary>
        /// <param name="unionName">Name of the union.</param>
        /// <param name="typesToInclude">The types to include.</param>
        public GraphUnionProxy(string unionName, IEnumerable<Type> typesToInclude)
        {
            this.Name = unionName?.Trim() ?? this.GetType().Name;
            this.Description = null;
            this.Types = new HashSet<Type>(typesToInclude);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUnionProxy"/> class.
        /// </summary>
        /// <param name="unionName">Name of the union.</param>
        /// <param name="typesToInclude">The types to include.</param>
        public GraphUnionProxy(string unionName, params Type[] typesToInclude)
            : this(unionName, typesToInclude as IEnumerable<Type>)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUnionProxy"/> class.
        /// </summary>
        /// <param name="typesToInclude">The types to include.</param>
        protected GraphUnionProxy(params Type[] typesToInclude)
            : this(null, typesToInclude as IEnumerable<Type>)
        {
        }

        /// <inheritdoc />
        public virtual Type ResolveType(Type runtimeObjectType)
        {
            return runtimeObjectType;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public HashSet<Type> Types { get; }

        /// <inheritdoc />
        public bool Publish => true;
    }
}