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
    /// A representation of a complex object type known to an <see cref="ISchema"/>.
    /// </summary>
    public interface IObjectGraphType : IGraphFieldContainer, IExtendableGraphType, IGraphInterfaceContainer
    {
    }
}