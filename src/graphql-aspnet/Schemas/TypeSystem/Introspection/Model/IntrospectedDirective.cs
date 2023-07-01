// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem.Introspection.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A model object containing data for a '__Directive' type created from a single
    /// <see cref="IDirective"/>.
    /// </summary>
    [DebuggerDisplay("DIRECTIVE: {Name}")]
    public sealed class IntrospectedDirective : IntrospectedItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedDirective" /> class.
        /// </summary>
        /// <param name="directiveType">The directive to inspect and build a data set for.</param>
        public IntrospectedDirective(IDirective directiveType)
            : base(directiveType)
        {
            this.Directive = directiveType;
        }

        /// <inheritdoc />
        public override void Initialize(IntrospectedSchema introspectedSchema)
        {
            var list = new List<IntrospectedInputValueType>();
            var directiveArguments = this.Directive.Arguments.Where(x => x.ArgumentModifiers.IsPartOfTheSchema());
            foreach (var arg in directiveArguments)
            {
                var introspectedType = introspectedSchema.FindIntrospectedType(arg.TypeExpression.TypeName);
                introspectedType = Introspection.WrapBaseTypeWithModifiers(introspectedType, arg.TypeExpression);
                var inputValue = new IntrospectedInputValueType(arg, introspectedType);
                list.Add(inputValue);
            }

            this.Arguments = list;
            this.Locations = this.Directive.Locations.GetIndividualFlags<DirectiveLocation>().ToList();
            this.Publish = this.Directive.Publish;
            this.IsRepeatable = this.Directive.IsRepeatable;
        }

        /// <summary>
        /// Gets the directive being introspected.
        /// </summary>
        /// <value>The directive.</value>
        private IDirective Directive { get; }

        /// <summary>
        /// Gets a collection of arguments this directive can accept on a query.
        /// </summary>
        /// <value>A collection of arguments assigned to this item.</value>
        public IReadOnlyList<IntrospectedInputValueType> Arguments { get; private set; }

        /// <summary>
        /// Gets the locations the target directive can be defined.
        /// </summary>
        /// <value>The locations.</value>
        public IReadOnlyList<DirectiveLocation> Locations { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IntrospectedDirective"/> is published
        /// in an introspection query.
        /// </summary>
        /// <value><c>true</c> if published; otherwise, <c>false</c>.</value>
        public bool Publish { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the target directive is repeatable at a target location.
        /// </summary>
        /// <value><c>true</c> if this directive is repeatable; otherwise, <c>false</c>.</value>
        public bool IsRepeatable { get; private set; }
    }
}