// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// A template outlining a directive to be invoked against a schema
    /// item.
    /// </summary>
    public class AppliedDirectiveTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirectiveTemplate" /> class.
        /// </summary>
        /// <param name="owner">The item to which the directive would be applied.</param>
        /// <param name="type">The type.</param>
        /// <param name="arguments">The arguments.</param>
        public AppliedDirectiveTemplate(IGraphItemTemplate owner, Type type, params object[] arguments)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            this.Directive = type;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Parses this template setting various properties needed to
        /// construct it.
        /// </summary>
        public virtual void Parse()
        {
        }

        /// <summary>
        /// Validates that this template is valid and correct
        /// or throws an exception.
        /// </summary>
        public virtual void ValidateOrThrow()
        {
            if (this.Directive == null || !Validation.IsCastable<GraphDirective>(this.Directive))
            {
                throw new GraphTypeDeclarationException(
                    "Invalid Applied Directive. A directive was assigned to " +
                    $"the item {this.Owner.InternalFullName} but the supplied type '{this.Directive?.GetType().FriendlyGraphTypeName() ?? "-null-"}' " +
                    $"is not a valid directive. All applied directive must inherit from {nameof(GraphDirective)}.");
            }
        }

        /// <summary>
        /// Gets the owner template of this directive invocation.
        /// </summary>
        /// <value>The owner.</value>
        public IGraphItemTemplate Owner { get; }

        /// <summary>
        /// Gets the directive to be invoked.
        /// </summary>
        /// <value>The directive.</value>
        public Type Directive { get; }

        /// <summary>
        /// Gets the argument values to pass to the directive when its invoked.
        /// </summary>
        /// <value>The arguments.</value>
        public object[] Arguments { get; }
    }
}