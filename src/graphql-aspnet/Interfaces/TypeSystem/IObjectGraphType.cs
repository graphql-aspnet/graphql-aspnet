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
    /// A representation of a complex object type known toa schema.
    /// </summary>
    public interface IObjectGraphType : IGraphType, IGraphFieldContainer, IExtendableGraphType, IGraphInterfaceContainer
    {
    }
}