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
    /// A base class attribute from which ALL graphql attributes inherit. Serves common functionality if needed
    /// as well as acts as a marker to determine if a class is decorated for graphql or not.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public abstract class BaseGraphAttribute : Attribute
    {
    }
}