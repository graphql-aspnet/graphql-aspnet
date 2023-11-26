// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas
{
    using System;

    /// <summary>
    /// A set of available strategy for setting not-null properties are
    /// various aspects of a type expression.
    /// </summary>
    [Flags]
    public enum GraphTypeExpressionNullabilityStrategies
    {
        None = 0,
        NonNullType = 1,
        NonNullLists = 2,
    }
}