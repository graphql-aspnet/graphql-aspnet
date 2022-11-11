// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Parsing2.Lexing.Tokens;

    /// <summary>
    /// A class representing the application of a <see cref="GraphDirective"/>
    /// to a schema item.
    /// </summary>
    [DebuggerDisplay("Directive = {DiagnosticName} (Arg Count = {ArgumentValues.Length})")]
    public class AppliedDirective : IAppliedDirective
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirective" /> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive to invoke.</param>
        /// <param name="arguments">The input arguments that will be used
        /// when this directive is invoked.</param>
        public AppliedDirective(Type directiveType, params object[] arguments)
        {
            this.DirectiveType = Validation.ThrowIfNullOrReturn(directiveType, nameof(directiveType));

            Validation.ThrowIfNotCastable<GraphDirective>(directiveType, nameof(directiveType));

            this.ArgumentValues = arguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirective" /> class.
        /// </summary>
        /// <param name="directiveName">Name of the directive as it will exist in the target schema.</param>
        /// <param name="arguments">The input arguments that will be used
        /// when this directive is invoked.</param>
        public AppliedDirective(string directiveName, params object[] arguments)
        {
            this.DirectiveName = Validation.ThrowIfNullWhiteSpaceOrReturn(directiveName, nameof(directiveName));
            if (this.DirectiveName.StartsWith(TokenTypeNames.STRING_AT_SYMBOL))
                this.DirectiveName = this.DirectiveName.Substring(1);

            this.ArgumentValues = arguments;
        }

        /// <inheritdoc />
        public IAppliedDirective Clone()
        {
            var arr = new object[this.ArgumentValues.Length];
            if (arr.Length > 0)
                Array.Copy(this.ArgumentValues, arr, arr.Length);

            if (this.DirectiveType != null)
                return new AppliedDirective(this.DirectiveType, arr);
            else
                return new AppliedDirective(this.DirectiveName, arr);
        }

        /// <inheritdoc />
        public Type DirectiveType { get; }

        /// <inheritdoc />
        public string DirectiveName { get; }

        /// <inheritdoc />
        public object[] ArgumentValues { get; }

        /// <summary>
        /// Gets a diagnostics friendly name that can be used in the debugger
        /// regardless of how this directive is applied.
        /// </summary>
        /// <value>The name of the diagnostic.</value>
        private string DiagnosticName
        {
            get
            {
                if (this.DirectiveType != null)
                    return this.DirectiveType.FriendlyName();
                else
                    return this.DirectiveName ?? string.Empty;
            }
        }
    }
}