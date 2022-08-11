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
    /// Marks this field as being "not required" on an INPUT_OBJECT in a query.
    /// When not supplied the <c>default</c> of the proeprty type, or a value set during the constructor,
    /// will be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotRequiredAttribute : BaseGraphAttribute
    {
    }
}