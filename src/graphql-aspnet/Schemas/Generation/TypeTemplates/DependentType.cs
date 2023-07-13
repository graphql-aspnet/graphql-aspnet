// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// An object describing another type which must exist in a schema for the owner
    /// to be correctly included.
    /// </summary>
    /// <remarks>
    /// If an object declares a property that returns a string, then that OBJECT graph type
    /// is said to be dependent on the String graph type for it to function correctly in
    /// a graph.
    /// </remarks>
    [DebuggerDisplay("{TypeName} (Kind = {ExpectedKind})")]
    public class DependentType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependentType"/> class.
        /// </summary>
        /// <param name="type">The .NET type that this instance references.</param>
        /// <param name="expectedKind">The expected type kind to declare the dependency
        /// if the type cannot be inferred from usage.</param>
        public DependentType(Type type, TypeKind expectedKind)
        {
            this.Type = Validation.ThrowIfNullOrReturn(type, nameof(type));
            this.ExpectedKind = expectedKind;
        }

        /// <summary>
        /// Gets the dependent type this instance points to.
        /// </summary>
        /// <value>The type.</value>
        public Type Type { get; }

        /// <summary>
        /// Gets the expected type kind that the target <see cref="Type"/> should be
        /// instantiated as.
        /// </summary>
        /// <value>The expected type kind of this dependent type.</value>
        public TypeKind ExpectedKind { get; }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        /// <value>The name of the type.</value>
        private string TypeName => this.Type?.FriendlyName() ?? "-null-";
    }
}