// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using System;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A template interface representing an SCALAR graph type.
    /// </summary>
    public interface IScalarGraphTypeTemplate : IGraphTypeTemplate
    {
        /// <summary>
        /// Gets the type declared as the scalar; the type that implements <see cref="IScalarGraphType"/>.
        /// </summary>
        /// <value>The type of the scalar.</value>
        Type ScalarType { get; }
    }
}