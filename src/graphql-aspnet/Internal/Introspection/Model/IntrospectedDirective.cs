﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Introspection.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A model object containing data for the __Directive type.
    /// </summary>
    [DebuggerDisplay("DIRECTIVE: {Name}")]
    public class IntrospectedDirective : IntrospectedItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedDirective" /> class.
        /// </summary>
        /// <param name="directiveType">Type of the directive.</param>
        public IntrospectedDirective(IDirective directiveType)
            : base(directiveType)
        {
            this.GraphType = directiveType;
        }

        /// <summary>
        /// When overridden in a child class,populates this introspected type using its parent schema to fill in any details about
        /// other references in this instance.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public override void Initialize(IntrospectedSchema schema)
        {
            var list = new List<IntrospectedInputValueType>();
            var directiveArguments = this.GraphType.Arguments.Where(x => !x.ArgumentModifiers.IsInternalParameter());
            foreach (var arg in directiveArguments)
            {
                var introspectedType = schema.FindIntrospectedType(arg.TypeExpression.TypeName);
                introspectedType = Introspection.WrapBaseTypeWithModifiers(introspectedType, arg.TypeExpression);
                var inputValue = new IntrospectedInputValueType(arg, introspectedType);
                list.Add(inputValue);
            }

            this.Arguments = list;
            this.Locations = this.GraphType.Locations.GetIndividualFlags<DirectiveLocation>().ToList();
            this.Publish = this.GraphType.Publish;
        }

        /// <summary>
        /// Gets the type of the graph.
        /// </summary>
        /// <value>The type of the graph.</value>
        protected IDirective GraphType { get; }

        /// <summary>
        /// Gets a collection of arguments this instance can accept on a query.
        /// </summary>
        /// <value>A collection of arguments assigned to this item.</value>
        public IReadOnlyList<IntrospectedInputValueType> Arguments { get; private set; }

        /// <summary>
        /// Gets the locations this directive can be defined.
        /// </summary>
        /// <value>The locations.</value>
        public IReadOnlyList<DirectiveLocation> Locations { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IntrospectedDirective"/> is published
        /// in an introspection query.
        /// </summary>
        /// <value><c>true</c> if published; otherwise, <c>false</c>.</value>
        public bool Publish { get; private set; }
    }
}