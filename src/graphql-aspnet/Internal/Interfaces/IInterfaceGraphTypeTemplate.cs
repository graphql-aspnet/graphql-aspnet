// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.Interfaces
{
    /// <summary>
    /// An interface representing a template that contains the necessary information to render an INTERFACE graph type
    /// for a schema.
    /// </summary>
    public interface IInterfaceGraphTypeTemplate : IGraphTypeTemplate, IGraphTypeFieldTemplateContainer
    {
    }
}