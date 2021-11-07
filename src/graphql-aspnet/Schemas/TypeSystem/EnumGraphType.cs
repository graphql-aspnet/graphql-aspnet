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
    using GraphQL.AspNet.Execution.ValueResolvers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.TypeSystem;

    /// <summary>
    /// A representation of any given enumeration (fixed set of values) in the type system.
    /// </summary>
    /// <seealso cref="IEnumGraphType" />
    [DebuggerDisplay("ENUM {Name}")]
    public class EnumGraphType : IEnumGraphType
    {
        private readonly Dictionary<string, IEnumOption> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumGraphType" /> class.
        /// </summary>
        /// <param name="name">The name to assign to this enumeration in the graph.</param>
        /// <param name="enumType">Type of the enum.</param>
        public EnumGraphType(string name, Type enumType)
            : this(name, enumType, new EnumLeafValueResolver(enumType))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumGraphType" /> class.
        /// </summary>
        /// <param name="name">The name to assign to this enumeration in the graph.</param>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="resolver">The resolver.</param>
        public EnumGraphType(string name, Type enumType, ILeafValueResolver resolver)
        {
            this.Name = Validation.ThrowIfNullEmptyOrReturn(name, nameof(name));
            this.ObjectType = Validation.ThrowIfNullOrReturn(enumType, nameof(enumType));
            this.SourceResolver = Validation.ThrowIfNullOrReturn(resolver, nameof(resolver));

            this.InternalName = this.ObjectType.FriendlyName();
            this.Publish = true;

            _options = new Dictionary<string, IEnumOption>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public virtual void AddOption(IEnumOption option)
        {
            Validation.ThrowIfNull(option, nameof(option));
            _options.Add(option.Name, option);
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
        public virtual IEnumOption RemoveOption(string name)
        {
            if (_options.ContainsKey(name))
            {
                var removedOption = _options[name];
                _options.Remove(name);
                return removedOption;
            }

            return null;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IEnumOption> Values => _options;

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public TypeKind Kind => TypeKind.ENUM;

        /// <inheritdoc />
        public virtual bool Publish { get; set; }

        /// <inheritdoc />
        public Type ObjectType { get; }

        /// <inheritdoc />
        public string InternalName { get; }

        /// <inheritdoc />
        public virtual bool IsVirtual => false;

        /// <inheritdoc />
        public ILeafValueResolver SourceResolver { get; set; }
    }
}