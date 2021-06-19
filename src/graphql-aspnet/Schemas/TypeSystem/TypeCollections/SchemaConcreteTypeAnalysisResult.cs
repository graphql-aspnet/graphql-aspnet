// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.TypeCollections
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A result generated from an operation involving an attempt to map one <see cref="Type"/> to another
    /// <see cref="Type"/> for a given <see cref="IGraphType"/>.
    /// </summary>
    [DebuggerDisplay("{GraphType.Name} (Type Count: {FoundTypes.Length})")]
    public class SchemaConcreteTypeAnalysisResult
    {
        private readonly Type[] _foundTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaConcreteTypeAnalysisResult"/> class.
        /// </summary>
        /// <param name="targetGraphType">Type of the target graph.</param>
        /// <param name="checkedType">Type of the checked.</param>
        /// <param name="foundTypes">The found types.</param>
        public SchemaConcreteTypeAnalysisResult(IGraphType targetGraphType, Type checkedType, params Type[] foundTypes)
        {
            this.GraphType = Validation.ThrowIfNullOrReturn(targetGraphType, nameof(targetGraphType));
            this.CheckedType = Validation.ThrowIfNullOrReturn(checkedType, nameof(checkedType));
            _foundTypes = foundTypes ?? new Type[0];
        }

        /// <summary>
        /// Gets the <see cref="IGraphType"/> that was inspected.
        /// </summary>
        /// <value>The graph type that was checked.</value>
        public IGraphType GraphType { get; }

        /// <summary>
        /// Gets the <see cref="Type"/> that was matched against.
        /// </summary>
        /// <value>The system type checked.</value>
        public Type CheckedType { get; }

        /// <summary>
        /// Gets all the found <see cref="Type"/> to which the <see cref="CheckedType"/> could masquerade as for the
        /// <see cref="GraphType"/>.
        /// </summary>
        /// <value>The found types which the <see cref="CheckedType"/> could masquerade as.</value>
        public Type[] FoundTypes => _foundTypes;

        /// <summary>
        /// Gets a value indicating whether a definitive, acceptable match was found for the <see cref="CheckedType"/>.
        /// </summary>
        /// <value><c>true</c> if this instance is successful; otherwise, <c>false</c>.</value>
        public bool ExactMatchFound => _foundTypes.Length == 1;
    }
}