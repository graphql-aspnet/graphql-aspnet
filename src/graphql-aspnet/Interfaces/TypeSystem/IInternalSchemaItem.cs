// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    /// <summary>
    /// <para>A marker interface used to identify some schema items as being part of the GraphQL.AspNet internal structural
    /// foundation to distinguish them from other, similarly typed schema items.
    /// </para>
    /// <para>
    /// e.g. an <see cref="IGraphOperation"/> is an <see cref="IObjectGraphType"/> but has
    /// special structural characteristics relevant to the library.
    /// </para>
    /// </summary>
    internal interface IInternalSchemaItem
    {
    }
}