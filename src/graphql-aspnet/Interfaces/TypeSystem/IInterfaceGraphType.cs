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
    /// A graph type representing an interface in the type system.
    /// </summary>
    public interface IInterfaceGraphType : IGraphFieldContainer, IExtendableGraphType
    {
    }
}