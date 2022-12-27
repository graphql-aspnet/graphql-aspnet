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
    using GraphQL.AspNet.Directives.Global;

    /// <summary>
    /// A specialized attribute that applies the built in @deprecated directive to
    /// the target field or enum value. Indicates that the target is deprecated and should not be
    /// used for any future query creation. This information will be exposed on any
    /// introspection queries targeting the field or enum value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    [DebuggerDisplay("{Reason}")]
    public class DeprecatedAttribute : ApplyDirectiveAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeprecatedAttribute"/> class.
        /// </summary>
        /// <param name="reason">An optional reason for the deprecation.</param>
        public DeprecatedAttribute(string reason = null)
            : base(typeof(DeprecatedDirective), reason?.Trim())
        {
            this.Reason = reason?.Trim();
        }

        /// <summary>
        /// Gets the reason for the depreciation of this field or value.
        /// </summary>
        /// <value>The reason.</value>
        public string Reason { get; }
    }
}