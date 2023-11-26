﻿// *************************************************************
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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A represention of a graphql object in the schema. This object defines all the exposed
    /// fields that can be "requested" when this type is queried.
    /// </summary>
    [DebuggerDisplay("INPUT {Name} (Fields = {Fields.Count})")]
    public class InputObjectGraphType : IInputObjectGraphType
    {
        private InputGraphFieldCollection _graphFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputObjectGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the graph type.</param>
        /// <param name="internalName">The internal name assigned to this graph type in source code.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="itemPath">The item path that identifies this object in the schema.</param>
        /// <param name="directives">The directives to apply to this input
        /// object when its added to a schema.</param>
        public InputObjectGraphType(
            string name,
            string internalName,
            Type objectType,
            ItemPath itemPath,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.ObjectType = Validation.ThrowIfNullOrReturn(objectType, nameof(objectType));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.ItemPath = Validation.ThrowIfNullOrReturn(itemPath, nameof(itemPath));
            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);
            this.Publish = true;

            _graphFields = new InputGraphFieldCollection();
        }

        /// <inheritdoc />
        public virtual IGraphType Clone(string typeName = null)
        {
            typeName = typeName?.Trim() ?? this.Name;
            var itemPath = this.ItemPath.Clone().Parent.CreateChild(typeName);

            var clonedItem = new InputObjectGraphType(
                typeName,
                this.InternalName,
                this.ObjectType,
                itemPath,
                this.AppliedDirectives);

            clonedItem.Description = this.Description;
            clonedItem.Publish = this.Publish;

            foreach (var field in this.Fields)
                this.AddField(field.Clone(clonedItem));

            return clonedItem;
        }

        /// <inheritdoc />
        public bool ValidateObject(object item)
        {
            return item == null || item.GetType() == this.ObjectType;
        }

        /// <summary>
        /// Attempts to add the field to the collection tracked by this graph type.
        /// </summary>
        /// <param name="field">The field.</param>
        public void AddField(IInputGraphField field)
        {
            _graphFields.AddField(field);
        }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <summary>
        /// Gets the collection of fields, keyed on their name, of all the fields nested or contained within this field.
        /// </summary>
        /// <value>The fields.</value>
        public IReadOnlyInputGraphFieldCollection Fields => _graphFields;

        /// <summary>
        /// Gets the <see cref="IGraphField"/> with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphTypeField.</returns>
        public IInputGraphField this[string fieldName] => this.Fields[fieldName];

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public bool Publish { get; set; }

        /// <inheritdoc />
        public bool IsVirtual => false;

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public ItemPath ItemPath { get; }

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.INPUT_OBJECT;
    }
}