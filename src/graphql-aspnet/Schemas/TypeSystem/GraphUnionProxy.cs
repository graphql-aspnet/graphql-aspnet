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
    using System.Diagnostics;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// An basic implementation of <see cref="IGraphUnionProxy"/> that can be
    /// inherited from for easy development.
    /// </summary>
    [DebuggerDisplay("UNION PROXY: {Name}")]
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
            this.Publish = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphUnionProxy"/> class.
        /// </summary>
        /// <param name="unionName">Name of the union as it should appear in the schema.</param>
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
        public virtual Type MapType(Type runtimeObjectType)
        {
            return runtimeObjectType;
        }

        /// <inheritdoc />
        public virtual string Name { get; set; }

        /// <inheritdoc />
        public virtual string Description { get; set; }

        /// <inheritdoc />
        public virtual HashSet<Type> Types { get; }

        /// <inheritdoc />
        public virtual bool Publish { get; set; }
    }
}