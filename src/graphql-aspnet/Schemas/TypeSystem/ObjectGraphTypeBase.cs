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
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A represention of a graphql object in the schema. This object defines all the exposed
    /// fields that can be "requested" when this type is queried.
    /// </summary>
    [DebuggerDisplay("OBJECT: {Name} (Fields = {Fields.Count})")]
    public abstract class ObjectGraphTypeBase : IGraphType
    {
        private readonly GraphFieldCollection _graphFields;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectGraphTypeBase" /> class.
        /// </summary>
        /// <param name="name">The name of the graph type as it is displayed in the __type information.</param>
        /// <param name="internalName">The defined internal name for this graph type.</param>
        /// <param name="itemPath">The item path of this object in the schema.</param>
        /// <param name="directives">The directives applied to this schema item
        /// when its added to a schema.</param>
        protected ObjectGraphTypeBase(
            string name,
            string internalName,
            ItemPath itemPath,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));
            this.ItemPath = Validation.ThrowIfNullOrReturn(itemPath, nameof(itemPath));
            _graphFields = new GraphFieldCollection();
            this.InterfaceNames = new HashSet<string>();
            this.Publish = true;

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc cref="IExtendableGraphType.Extend" />
        public virtual IGraphField Extend(IGraphField newField)
        {
            Validation.ThrowIfNull(newField, nameof(newField));
            if (newField.Parent != this)
                newField = newField.Clone(this);

            return this.GraphFieldCollection.AddField(newField);
        }

        /// <inheritdoc />
        public abstract bool ValidateObject(object item);

        /// <inheritdoc />
        public abstract IGraphType Clone(string typeName = null);

        /// <summary>
        /// Gets the mutatable collection of graph fields.
        /// </summary>
        /// <value>The graph field collection.</value>
        protected GraphFieldCollection GraphFieldCollection => _graphFields;

        /// <summary>
        /// Gets the collection of fields, keyed on their name, of all the fields nested or contained within this field.
        /// </summary>
        /// <value>The fields.</value>
        public IReadOnlyGraphFieldCollection Fields => _graphFields;

        /// <summary>
        /// Gets the <see cref="IGraphField"/> with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphTypeField.</returns>
        public virtual IGraphField this[string fieldName] => this.Fields[fieldName];

        /// <inheritdoc />
        public virtual string Name { get; set; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public virtual string Description { get; set; }

        /// <inheritdoc />
        public virtual TypeKind Kind => TypeKind.OBJECT;

        /// <summary>
        /// Gets the internal collection of interfaces this object graph type implements.
        /// </summary>
        /// <value>The interfaces.</value>
        public HashSet<string> InterfaceNames { get; }

        /// <inheritdoc />
        public virtual bool Publish { get; set; }

        /// <inheritdoc />
        public virtual bool IsVirtual => false;

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public ItemPath ItemPath { get; }
    }
}