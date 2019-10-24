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
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;

    /// <summary>
    /// A qualified option on an enumeration.
    /// </summary>
    [DebuggerDisplay("Value = {Name}")]
    public class GraphEnumOption : IEnumOption
    {
        private readonly Type _parentEnum;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphEnumOption" /> class.
        /// </summary>
        /// <param name="parentEnum">The parent enum that owns this option.</param>
        /// <param name="name">The value.</param>
        /// <param name="description">The description.</param>
        /// <param name="isExplicitlyDeclared">if set to <c>true</c> denotes this enum option was explicitly declared to be part of the object graph.</param>
        /// <param name="isDeprecated">if set to <c>true</c> this option is considred deprecated and marked for removal.</param>
        /// <param name="deprecationReason">The deprecation reason.</param>
        public GraphEnumOption(Type parentEnum, string name, string description, bool isExplicitlyDeclared, bool isDeprecated = false, string deprecationReason = null)
        {
            _parentEnum = parentEnum;
            this.Name = Validation.ThrowIfNullEmptyOrReturn(name, nameof(name));
            this.Description = description?.Trim();
            this.IsDeprecated = isDeprecated;
            this.DeprecationReason = deprecationReason?.Trim();
            this.IsExplicitlyDeclared = isExplicitlyDeclared;
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public void ValidateOrThrow()
        {
            GraphValidation.EnsureGraphNameOrThrow($"{_parentEnum.Name}", this.Name);
        }

        /// <summary>
        /// Gets a value indicating whether this instance was explicitly declared to be part of the object graph or not.
        /// </summary>
        /// <value><c>true</c> if this instance is explicitly declared; otherwise, <c>false</c>.</value>
        public bool IsExplicitlyDeclared { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        /// <summary>
        /// Gets a value indicating whether this item  is depreciated. The <see cref="DeprecationReason" /> will be displayed
        /// on any itnrospection requests.
        /// </summary>
        /// <value><c>true</c> if this instance is depreciated; otherwise, <c>false</c>.</value>
        public bool IsDeprecated { get; }

        /// <summary>
        /// Gets the provided reason for this item being depreciated.
        /// </summary>
        /// <value>The depreciation reason.</value>
        public string DeprecationReason { get; }
    }
}