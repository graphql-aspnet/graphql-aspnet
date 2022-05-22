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
    /// An interface describing data to fully populate an enumeration item into the object graph.
    /// </summary>
    public interface IEnumValue : ISchemaItem, IDeprecatable
    {
    }
}