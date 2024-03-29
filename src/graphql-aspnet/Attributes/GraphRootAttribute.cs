﻿// *************************************************************
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
    /// An attribute applied to controllers to denote that the controller's named field will
    /// be excluded on the object graph and any action methods contained within this controller
    /// will be assigned directly to their root operations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphRootAttribute : GraphRouteAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRootAttribute"/> class.
        /// </summary>
        public GraphRootAttribute()
         : base(true)
        {
        }
    }
}