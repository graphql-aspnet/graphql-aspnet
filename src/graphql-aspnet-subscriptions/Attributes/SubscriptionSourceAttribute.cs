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
    /// A marker attribute to desginate a parameter of a subscription method as being the
    /// expected source data for this subscription field. This parameter is not exposed
    /// on the schema.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class SubscriptionSourceAttribute : Attribute
    {
    }
}