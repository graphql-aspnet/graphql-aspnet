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
        /// <param name="introspectedFieldOwner">The introspected object representing the graph type returned
        /// by this field.</param>
        public IntrospectedField(IGraphField field, IntrospectedType introspectedFieldOwner)
            : base(field)
        {
            this.IntrospectedGraphType = Validation.ThrowIfNullOrReturn(introspectedFieldOwner, nameof(introspectedFieldOwner));
            _field = Validation.ThrowIfNullOrReturn(field, nameof(field));
            this.IsDeprecated = _field.IsDeprecated;
            this.DeprecationReason = _field.DeprecationReason;
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

        /// <summary>
        /// Gets a collection of arguments this instance can accept on a query.
        /// </summary>
        /// <value>A collection of arguments assigned to this item.</value>
        public IReadOnlyList<IntrospectedInputValueType> Arguments { get; private set; }

        /// <inheritdoc />
        public bool IsDeprecated { get; set; }

        /// <inheritdoc />
        public string DeprecationReason { get; set; }
    }
}