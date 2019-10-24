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
    public class IntrospectedField : IntrospectedItem, INamedItem, IDeprecatable
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
        }

        /// <summary>
        /// When overridden in a child class,populates this introspected type using its parent schema to fill in any details about
        /// other references in this instance.
        /// </summary>
        /// <param name="schema">The schema.</param>
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
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public string Name => _field.Name;

        /// <summary>
        /// Gets a collection of arguments this instance can accept on a query.
        /// </summary>
        /// <value>A collection of arguments assigned to this item.</value>
        public IReadOnlyList<IntrospectedInputValueType> Arguments { get; private set; }

        /// <summary>
        /// Gets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public string Description => _field.Description;

        /// <summary>
        /// Gets a value indicating whether this action method is depreciated. The <see cref="DeprecationReason"/> will be displayed
        /// on any itnrospection requests.
        /// </summary>
        /// <value><c>true</c> if this instance is depreciated; otherwise, <c>false</c>.</value>
        public bool IsDeprecated => _field.IsDeprecated;

        /// <summary>
        /// Gets the provided reason for this item being depreciated.
        /// </summary>
        /// <value>The depreciation reason.</value>
        public string DeprecationReason => _field.DeprecationReason;
    }
}