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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A model object containing data for a '__Field' type
    /// created from one field in a graph type.
    /// </summary>
    [DebuggerDisplay("field: {Name}")]
    public sealed class IntrospectedField : IntrospectedItem, ISchemaItem
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
        }

        /// <inheritdoc />
        public override void Initialize(IntrospectedSchema introspectedSchema)
        {
            var list = new List<IntrospectedInputValueType>();
            foreach (var arg in _field.Arguments)
            {
                var introspectedType = introspectedSchema.FindIntrospectedType(arg.TypeExpression.TypeName);
                introspectedType = Introspection.WrapBaseTypeWithModifiers(introspectedType, arg.TypeExpression);
                if(introspectedType == null)
                {
                    var str = "";
                }

                var inputValue = new IntrospectedInputValueType(arg, introspectedType);
                inputValue.Initialize(introspectedSchema);
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

        /// <summary>
        /// Gets a value indicating whether the field this introspection item represents
        /// is deprecated.
        /// </summary>
        /// <value><c>true</c> if this instance is deprecated; otherwise, <c>false</c>.</value>
        public bool IsDeprecated => _field.IsDeprecated;

        /// <summary>
        /// Gets the reason, if any, why the field this introspection item represents
        /// was deprecated.
        /// </summary>
        /// <value>The reason the target field was deprecated.</value>
        public string DeprecationReason => _field.DeprecationReason;
    }
}