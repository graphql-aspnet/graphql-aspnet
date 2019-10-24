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
    /// A marker for a class, interface, method, property enum or enum value to denote that the graphql templating
    /// engine should ignore it completely. In the case of a class or interface, an exception will be thrown by the runtime
    /// if it attempts to be ingested by a schema as a graph type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Enum
        | AttributeTargets.Class)]
    public class GraphSkipAttribute : Attribute
    {
    }
}