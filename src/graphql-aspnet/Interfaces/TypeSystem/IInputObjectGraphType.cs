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
    /// A representation of a complex object type known to a schema, used in an "input" scenario.
    /// </summary>
    public interface IInputObjectGraphType : IGraphType, ITypedItem, IGraphFieldContainer
    {
    }
}