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
    using System.Diagnostics;

    /// <summary>
    /// Indicates that the graph field is depreciated and should not be used for any future query creation.
    /// Depreciated fields are likely to removed at a future date.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    [DebuggerDisplay("{Reason}")]
    public class DeprecatedAttribute : BaseGraphAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeprecatedAttribute"/> class.
        /// </summary>
        /// <param name="reason">An optional reason for the deprecation.</param>
        public DeprecatedAttribute(string reason = null)
        {
            this.Reason = reason?.Trim();
        }

        /// <summary>
        /// Gets the reason for the depreciation.
        /// </summary>
        /// <value>The reason.</value>
        public string Reason { get; }
    }
}