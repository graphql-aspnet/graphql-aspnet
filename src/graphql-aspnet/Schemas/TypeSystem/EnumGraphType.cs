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
        {
            this.Name = Validation.ThrowIfNullEmptyOrReturn(name, nameof(name));
            this.ObjectType = Validation.ThrowIfNullOrReturn(enumType, nameof(enumType));
            this.InternalName = this.ObjectType.FriendlyName();
            this.Publish = true;
            _options = new Dictionary<string, IEnumOption>();
        }

        /// <summary>
        /// Adds the option.
        /// </summary>
        /// <param name="option">The option.</param>
        public void AddOption(IEnumOption option)
        {
            Validation.ThrowIfNull(option, nameof(option));
            _options.Add(option.Name, option);
        }

        /// <summary>
        /// Determines whether the provided item is of a concrete type represented by this graph type.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is of the correct type; otherwise, <c>false</c>.</returns>
        public bool ValidateObject(object item)
        {
            if (item == null)
                return true;

            if (!(item is Enum))
                return false;

            return Enum.IsDefined(this.ObjectType, item);
        }

        /// <summary>
        /// Gets the values that can be handled by this enumeration.
        /// </summary>
        /// <value>The values.</value>
        public IReadOnlyDictionary<string, IEnumOption> Values => _options;

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
        public TypeKind Kind => TypeKind.ENUM;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IGraphType" /> is published on an introspection request.
        /// </summary>
        /// <value><c>true</c> if publish; otherwise, <c>false</c>.</value>
        public virtual bool Publish { get; set; }

        /// <summary>
        /// Gets the type of the object this graph type was made from.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType { get; }

        /// <summary>
        /// Gets a fully qualified name of the type as it exists on the server (i.e.  Namespace.ClassName). This name
        /// is used in many exceptions and internal error messages.
        /// </summary>
        /// <value>The name of the internal.</value>
        public string InternalName { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is virtual and added by the runtime to facilitate
        /// a user defined graph structure. When false, this graph types points to a concrete type
        /// defined by a developer.
        /// </summary>
        /// <value><c>true</c> if this instance is virtual; otherwise, <c>false</c>.</value>
        public virtual bool IsVirtual => false;
    }
}