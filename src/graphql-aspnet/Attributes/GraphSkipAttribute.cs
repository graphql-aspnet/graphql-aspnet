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
    /// to graphql that it should be ignored. This attribute has priority over all other
    /// attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method
        | AttributeTargets.Property
        | AttributeTargets.Field
        | AttributeTargets.Enum
        | AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Parameter)]
    public class GraphSkipAttribute : Attribute
    {
        // Implementation note: This attribute purposefully does
        // not inherit from BaseGraphAttribute. The intent behind BaseGraphAttribute
        // is to attribute things which could be easily identified as an item as being
        // "part of GraphQL".  This attribute, however; exclusively marks an item
        // as being NOT part of GraphQL.
    }
}