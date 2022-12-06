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
    /// A marker interface to identify <see cref="ISchemaItem"/>
    /// objects that are part of the introspection system.
    /// </summary>
    public interface IIntrospectionSchemaItem : ISchemaItem
    {
    }
}