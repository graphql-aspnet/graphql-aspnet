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
    /// A base attribute from which ALL graphql attributes inherit.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public abstract class BaseGraphAttribute : Attribute
    {
    }
}