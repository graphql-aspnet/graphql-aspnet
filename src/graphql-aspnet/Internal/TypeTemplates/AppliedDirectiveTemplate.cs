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
            this.DirectiveType = type;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirectiveTemplate" /> class.
        /// </summary>
        /// <param name="owner">The item to which the directive would be applied.</param>
        /// <param name="directiveName">Name of the directive as it will appear in the target schema.</param>
        /// <param name="arguments">The arguments.</param>
        public AppliedDirectiveTemplate(INamedItem owner, string directiveName, params object[] arguments)
        {
            this.Owner = Validation.ThrowIfNullOrReturn(owner, nameof(owner));
            this.DirectiveName = directiveName;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirectiveTemplate" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="arguments">The arguments.</param>
        public AppliedDirectiveTemplate(Type type, params object[] arguments)
        {
            this.DirectiveType = type;
            this.Arguments = arguments;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppliedDirectiveTemplate" /> class.
        /// </summary>
        /// <param name="directiveName">Name of the directive as it will appear in the target schema.</param>
        /// <param name="arguments">The arguments.</param>
        public AppliedDirectiveTemplate(string directiveName, params object[] arguments)
        {
            this.DirectiveName = directiveName;
            this.Arguments = arguments;
        }

        /// <inheritdoc />
        public virtual void Parse()
        {
            if (this.DirectiveName != null)
            {
                this.DirectiveName = this.DirectiveName.Trim();
                while (this.DirectiveName.StartsWith("@"))
                    this.DirectiveName = this.DirectiveName.Substring(1);
            }
        }

        /// <inheritdoc />
        public virtual void ValidateOrThrow()
        {
            if (string.IsNullOrWhiteSpace(this.DirectiveName) && this.DirectiveType == null)
            {
                throw new GraphTypeDeclarationException(
                    "Invalid Applied Directive. A directive was assigned to " +
                    $"the item {this.Owner.Name} but both the expected type " +
                    $"and the expected directive name were null. Either a type or a name must be supplied.");
            }

            if (this.DirectiveType != null && !Validation.IsCastable<GraphDirective>(this.DirectiveType))
            {
                throw new GraphTypeDeclarationException(
                    "Invalid Applied Directive. A directive was assigned to " +
                    $"the item {this.Owner.Name} but the supplied type '{this.DirectiveType?.GetType().FriendlyGraphTypeName() ?? "-null-"}' " +
                    $"is not a valid directive. All applied directive must inherit from {nameof(GraphDirective)}.");
            }
        }

        /// <inheritdoc />
        public virtual IAppliedDirective CreateAppliedDirective()
        {
            if (this.DirectiveType != null)
                return new AppliedDirective(this.DirectiveType, this.Arguments);
            else
                return new AppliedDirective(this.DirectiveName, this.Arguments);
        }

        /// <inheritdoc />
        public INamedItem Owner { get; }

        /// <inheritdoc />
        public Type DirectiveType { get; }

        /// <inheritdoc />
        public string DirectiveName { get; private set; }

        /// <inheritdoc />
        public object[] Arguments { get; }
    }
}