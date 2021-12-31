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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A template outlining a directive to be invoked against a schema
    /// item.
    /// </summary>
    public class AppliedDirectiveTemplate : IAppliedDirectiveTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirectiveTemplate" /> class.
        /// </summary>
        /// <param name="owner">The item to which the directive would be applied.</param>
        /// <param name="type">The type.</param>
        /// <param name="arguments">The arguments.</param>
        public AppliedDirectiveTemplate(INamedItem owner, Type type, params object[] arguments)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            this.Directive = type;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirectiveTemplate" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="arguments">The arguments.</param>
        public AppliedDirectiveTemplate(Type type, params object[] arguments)
        {
            this.Directive = type;
            this.Arguments = arguments;
        }

        /// <inheritdoc />
        public virtual void Parse()
        {
        }

        /// <inheritdoc />
        public virtual void ValidateOrThrow()
        {
            if (this.Directive == null || !Validation.IsCastable<GraphDirective>(this.Directive))
            {
                throw new GraphTypeDeclarationException(
                    "Invalid Applied Directive. A directive was assigned to " +
                    $"the item {this.Owner.Name} but the supplied type '{this.Directive?.GetType().FriendlyGraphTypeName() ?? "-null-"}' " +
                    $"is not a valid directive. All applied directive must inherit from {nameof(GraphDirective)}.");
            }
        }

        /// <inheritdoc />
        public virtual IAppliedDirective CreateAppliedDirective()
        {
            return new AppliedDirective(this.Directive, this.Arguments);
        }

        /// <inheritdoc />
        public INamedItem Owner { get; }

        /// <inheritdoc />
        public Type Directive { get; }

        /// <inheritdoc />
        public object[] Arguments { get; }
    }
}