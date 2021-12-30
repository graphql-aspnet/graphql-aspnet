// *************************************************************
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
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A model object containing data for the __Field type of one field in a graph type.
    /// </summary>
    [DebuggerDisplay("field: {Name}")]
    public class IntrospectedField : IntrospectedItem, ISchemaItem, IDeprecatable
    {
        private readonly IGraphField _field;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntrospectedField" /> class.
        /// </summary>
        /// <param name="field">The field itself.</param>
        /// <param name="introspectedType">The introspected object representing the graph type returned
        /// by this field.</param>
        public IntrospectedField(IGraphField field, IntrospectedType introspectedType)
        {
            this.IntrospectedGraphType = Validation.ThrowIfNullOrReturn(introspectedType, nameof(introspectedType));
            _field = Validation.ThrowIfNullOrReturn(field, nameof(field));
            this.AppliedDirectives = new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc />
        public override void Initialize(IntrospectedSchema schema)
        {
            var list = new List<IntrospectedInputValueType>();
            foreach (var arg in _field.Arguments.Where(x => !x.ArgumentModifiers.HasFlag(GraphArgumentModifiers.Internal)))
            {
                var introspectedType = schema.FindIntrospectedType(arg.TypeExpression.TypeName);
                introspectedType = Introspection.WrapBaseTypeWithModifiers(introspectedType, arg.TypeExpression);
                var inputValue = new IntrospectedInputValueType(arg, introspectedType);
                inputValue.Initialize(schema);
                list.Add(inputValue);
            }

            this.Arguments = list;
        }

        /// <summary>
        /// Gets the graph type returned by this field.
        /// </summary>
        /// <value>The type of the graph.</value>
        public IntrospectedType IntrospectedGraphType { get; }

        /// <inheritdoc />
        public string Name => _field.Name;

        /// <summary>
        /// Gets a collection of arguments this instance can accept on a query.
        /// </summary>
        /// <value>A collection of arguments assigned to this item.</value>
        public IReadOnlyList<IntrospectedInputValueType> Arguments { get; private set; }

        /// <inheritdoc />
        public string Description => _field.Description;

        /// <inheritdoc />
        public bool IsDeprecated => _field.IsDeprecated;

        /// <inheritdoc />
        public string DeprecationReason => _field.DeprecationReason;

        /// <inheritdoc />
        [GraphSkip]
        public IAppliedDirectiveCollection AppliedDirectives { get; }
    }
}