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
    /// <para>An attribute applied to controllers to denote the route path to the instance within the resultant object graph.</para>
    ///
    /// <para>Note: Variable parameter specifications using curly braces (e.g. 'path/{variable}') are not permitted on graph routes. The entire graph structure
    /// must be known and evaluated to properly validate a query aimed at a target schema.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphRouteAttribute : BaseGraphAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRouteAttribute"/> class.
        /// </summary>
        /// <param name="template">The route template that represents this instance.  (e.g. '/people/heros').</param>
        public GraphRouteAttribute(string template)
        {
            this.Template = template?.Trim();
            this.IgnoreControllerField = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphRouteAttribute"/> class.
        /// </summary>
        /// <param name="ignoreControllerField">if set to <c>true</c> this controller will be excluded as a field
        /// on the object graph and any action methods contained within it will be assigne directly to their root operations.</param>
        protected GraphRouteAttribute(bool ignoreControllerField)
        {
            this.IgnoreControllerField = ignoreControllerField;
        }

        /// <summary>
        /// Gets the template assigned to this instance.
        /// </summary>
        /// <value>The route fragment.</value>
        public string Template { get; }

        /// <summary>
        /// Gets a value indicating whether this controller will not be included in the object graph and will instead
        /// have its action methods assigned directly to their graph operations.
        /// </summary>
        /// <value><c>true</c> if the controller field should be ignored; otherwise, <c>false</c>.</value>
        public bool IgnoreControllerField { get; }
    }
}