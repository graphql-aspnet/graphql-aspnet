// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Scalars
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// An internal reference to the primary details about a scalar used for
    /// categorizing and referencing it at runtime.
    /// </summary>
    internal class ScalarReference
    {
        /// <summary>
        /// Creates a reference of the scalar type to be used for indexing
        /// and searching.
        /// </summary>
        /// <param name="graphType">An instance of the graph type from which reference
        /// details can be extracted.</param>
        /// <param name="instanceType">The concrete type from which
        /// instances of the scalar type can be created This is NOT the .NET type representing
        /// the scalar, but rather the registered system type from which
        /// <paramref name="graphType"/> is created.</param>
        /// <returns>ScalarReference.</returns>
        public static ScalarReference Create(IScalarGraphType graphType, Type instanceType)
        {
            Validation.ThrowIfNull(graphType, nameof(graphType));
            Validation.ThrowIfNull(instanceType, nameof(instanceType));

            var reference = new ScalarReference();
            reference.InstanceType = instanceType;
            reference.PrimaryType = graphType.ObjectType;

            if (graphType.OtherKnownTypes.Count > 0)
                reference.OtherKnownTypes = graphType.OtherKnownTypes.ToList();
            else
                reference.OtherKnownTypes = new List<Type>();

            reference.Name = graphType.Name;
            return reference;
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="ScalarReference"/> class from being created.
        /// </summary>
        private ScalarReference()
        {
        }

        /// <summary>
        /// Gets the registered type from which instances of the
        /// scalar can be made (e.g. LongScalarType, StringScalarType etc.).
        /// </summary>
        /// <value>The type of the instance class for this scalar.</value>
        public Type InstanceType { get; private set; }

        /// <summary>
        /// Gets the primary .NET type represented by the scalar. (e.g. int, string, long etc.)
        /// </summary>
        /// <value>The type of the primary.</value>
        public Type PrimaryType { get; private set; }

        /// <summary>
        /// Gets a list of known alternate .NET types that can be
        /// handled by this scalar (e.g. int?, long? etc.)
        /// </summary>
        /// <value>The other known types.</value>
        public IReadOnlyList<Type> OtherKnownTypes { get; private set; }

        /// <summary>
        /// Gets the name of this scalar as it has been declared.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }
    }
}