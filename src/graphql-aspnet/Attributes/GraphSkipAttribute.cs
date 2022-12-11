// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Attributes
{
    using System;

    /// <summary>
    /// When applied to a class, interface, method, property enum or enum value, indicates
    /// to graphql that it should be ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Enum
        | AttributeTargets.Class
        | AttributeTargets.Struct)]
    public class GraphSkipAttribute : Attribute
    {
    }
}