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
    using GraphQL.AspNet.Common.Extensions;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceGraphType" /> class.
        /// </summary>
        /// <param name="name">The name of the interface as it will appear in the schema.</param>
        /// <param name="concreteType">The concrete type representing this interface.</param>
        /// <param name="route">The route path that identifies this interface.</param>
        /// <param name="directives">The directives to apply to this type
        /// when its added to a schema.</param>
        public InterfaceGraphType(
            string name,
            Type concreteType,
            SchemaItemPath route,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullOrReturn(name, nameof(name));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.ObjectType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
            this.InternalName = this.ObjectType.FriendlyName();
            this.InterfaceNames = new HashSet<string>();

            _fieldSet = new GraphFieldCollection(this);

            this.Publish = true;

            this.Extend(new Introspection_TypeNameMetaField(name));

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);
        }

        /// <inheritdoc />
        public IGraphField Extend(IGraphField newField)
        {
            return _fieldSet.AddField(newField);
        }

        /// <inheritdoc />
        public bool ValidateObject(object item)
        {
            return item == null || Validation.IsCastable(item.GetType(), this.ObjectType);
        }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.INTERFACE;

        /// <inheritdoc />
        public bool Publish { get; set; }

        /// <inheritdoc />
        public IReadOnlyGraphFieldCollection Fields => _fieldSet;

        /// <summary>
        /// Gets the <see cref="IGraphField"/> with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphTypeField.</returns>
        public IGraphField this[string fieldName] => this.Fields[fieldName];

        /// <inheritdoc />
        public virtual bool IsVirtual => false;

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public virtual string InternalName { get; }

        /// <inheritdoc />
        public HashSet<string> InterfaceNames { get; }
    }
}