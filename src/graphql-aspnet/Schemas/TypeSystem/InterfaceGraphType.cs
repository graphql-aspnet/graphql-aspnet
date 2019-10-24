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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Introspection.Fields;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// An implementation of a metadata container for the interface graphtype.
    /// </summary>
    [DebuggerDisplay("INTERFACE {Name} (Fields = {Fields.Count})")]
    public class InterfaceGraphType : IInterfaceGraphType
    {
        private readonly GraphFieldCollection _fieldSet;
        private readonly Type _interfaceType;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceGraphType" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="concreteType">The concrete type representing this interface.</param>
        /// <param name="fields">The fields that define the interface.</param>
        public InterfaceGraphType(string name, Type concreteType, IEnumerable<IGraphField> fields)
        {
            this.Name = Validation.ThrowIfNullOrReturn(name, nameof(name));
            _fieldSet = new GraphFieldCollection();
            _interfaceType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
            this.Publish = true;
            if (fields != null)
            {
                foreach (var field in fields)
                    this.Extend(field);
            }

            this.Extend(new Introspection_TypeNameMetaField(name));
        }

        /// <summary>
        /// Extends the specified new field.
        /// </summary>
        /// <param name="newField">The new field.</param>
        public void Extend(IGraphField newField)
        {
            _fieldSet.AddField(newField);
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public bool ValidateObject(object item)
        {
            return item == null || Validation.IsCastable(item.GetType(), _interfaceType);
        }

        /// <summary>
        /// Gets the formal name of this item as it exists in the object graph.
        /// </summary>
        /// <value>The publically referenced name of this field in the graph.</value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the human-readable description distributed with this field
        /// when requested. The description should accurately describe the contents of this field
        /// to consumers.
        /// </summary>
        /// <value>The publically referenced description of this field in the type system.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the value indicating what type of graph type this instance is in the type system. (object, scalar etc.)
        /// </summary>
        /// <value>The kind.</value>
        public TypeKind Kind => TypeKind.INTERFACE;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphType"/> is published on an introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public bool Publish { get; set; }

        /// <summary>
        /// Gets a collection of fields made available by this interface.
        /// </summary>
        /// <value>The fields.</value>
        public IReadOnlyGraphFieldCollection Fields => _fieldSet;

        /// <summary>
        /// Gets the <see cref="IGraphField"/> with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphTypeField.</returns>
        public IGraphField this[string fieldName] => this.Fields[fieldName];
    }
}