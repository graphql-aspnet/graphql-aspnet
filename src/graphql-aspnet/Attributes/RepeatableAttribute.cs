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
    using GraphQL.AspNet.Directives;

    /// <summary>
    /// An attribute that can be applied to a <see cref="GraphDirective"/> to indicate
    /// that the directive is repeatable on a given target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RepeatableAttribute : GraphAttributeBase
    {
    }
}