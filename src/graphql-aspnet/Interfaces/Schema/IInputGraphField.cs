// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Schema
{
    /// <summary>
    /// A field of data on an INPUT_OBJECT graph type.
    /// </summary>
    public interface IInputGraphField : IGraphFieldBase, IDefaultValueSchemaItem, ITypedSchemaItem, IDeprecatable
    {
    }
}