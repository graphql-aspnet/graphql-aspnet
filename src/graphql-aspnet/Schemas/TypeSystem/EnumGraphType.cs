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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A representation of any given enumeration (fixed set of values) in the type system.
    /// </summary>
    /// <seealso cref="IEnumGraphType" />
    [DebuggerDisplay("ENUM {Name}")]
    public class EnumGraphType : IEnumGraphType
    {
        private EnumValueCollection _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumGraphType" /> class.
        /// </summary>
        /// <param name="name">The name to assign to this enumeration in the graph.</param>
        /// <param name="internalName">The internal name of this graph type as its assigned in source code.</param>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="itemPath">The item path that identifies this enum type.</param>
        /// <param name="directives">The directives to apply to this enum type.</param>
        public EnumGraphType(
            string name,
            string internalName,
            Type enumType,
            ItemPath itemPath,
            IAppliedDirectiveCollection directives = null)
            : this(name, internalName, enumType, itemPath, new EnumLeafValueResolver(enumType), directives)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumGraphType" /> class.
        /// </summary>
        /// <param name="name">The name to assign to this enumeration in the graph.</param>
        /// <param name="internalName">The internal name of this graph type as its assigned in source code.</param>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="itemPath">The item path that identifies this enum type.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="directives">The directives to apply to this enum type.</param>
        public EnumGraphType(
            string name,
            string internalName,
            Type enumType,
            ItemPath itemPath,
            ILeafValueResolver resolver,
            IAppliedDirectiveCollection directives = null)
        {
            this.Name = Validation.ThrowIfNullWhiteSpaceOrReturn(name, nameof(name));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));

            this.ObjectType = Validation.ThrowIfNullOrReturn(enumType, nameof(enumType));
            this.ItemPath = Validation.ThrowIfNullOrReturn(itemPath, nameof(itemPath));
            this.SourceResolver = Validation.ThrowIfNullOrReturn(resolver, nameof(resolver));

            this.Publish = true;

            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);

            _options = new EnumValueCollection(this);
        }

        /// <inheritdoc />
        public virtual void AddOption(IEnumValue option)
        {
            Validation.ThrowIfNull(option, nameof(option));
            _options.Add(option);
        }

        /// <inheritdoc />
        public virtual IEnumValue RemoveOption(string name)
        {
            return _options.Remove(name);
        }

        /// <inheritdoc />
        public virtual bool ValidateObject(object item)
        {
            if (item == null)
                return true;

            if (!(item is Enum))
                return false;

            return Enum.IsDefined(this.ObjectType, item);
        }

        /// <inheritdoc />
        public virtual IGraphType Clone(string typeName = null)
        {
            typeName = typeName?.Trim() ?? this.Name;

            var clonedItem = new EnumGraphType(
                typeName,
                this.InternalName,
                this.ObjectType,
                this.ItemPath.Parent.CreateChild(typeName),
                this.AppliedDirectives);

            clonedItem.Description = this.Description;
            clonedItem.Publish = this.Publish;

            foreach (var enumValue in this.Values)
                clonedItem.AddOption(enumValue.Value.Clone(clonedItem));

            return clonedItem;
        }

        /// <inheritdoc />
        public virtual IEnumValueCollection Values => _options;

        /// <inheritdoc />
        public virtual string Name { get; set; }

        /// <inheritdoc />
        public virtual string Description { get; set; }

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.ENUM;

        /// <inheritdoc />
        public virtual bool Publish { get; set; }

        /// <inheritdoc />
        public virtual Type ObjectType { get; }

        /// <inheritdoc />
        public virtual string InternalName { get; }

        /// <inheritdoc />
        public virtual bool IsVirtual => false;

        /// <inheritdoc />
        public virtual ILeafValueResolver SourceResolver { get; set; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public ItemPath ItemPath { get; }
    }
}