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
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// A qualified option on a published ENUM graph type.
    /// </summary>
    [DebuggerDisplay("Value = {Name}")]
    public class EnumValue : IEnumValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValue" /> class.
        /// </summary>
        /// <param name="parent">The parent enum graph type that owns this value.</param>
        /// <param name="name">The value.</param>
        /// <param name="description">The description.</param>
        /// <param name="route">The route path that uniquely identifies this enum option.</param>
        /// <param name="internalValue">The value of the enum as its declared in .NET.</param>
        /// <param name="declaredLabel">A string representation of label declared on the enum value in .NET.</param>
        /// <param name="internalName">The internal name assigned to this enum value. Typically the same as <paramref name="declaredLabel"/>
        /// but can be customized by the developer for reporting purposes.</param>
        /// <param name="directives">The set of directives to execute
        /// against this option when it is added to the schema.</param>
        public EnumValue(
            IEnumGraphType parent,
            string name,
            string description,
            SchemaItemPath route,
            object internalValue,
            string declaredLabel,
            string internalName,
            IAppliedDirectiveCollection directives = null)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            this.Name = Validation.ThrowIfNullEmptyOrReturn(name, nameof(name));
            this.Route = Validation.ThrowIfNullOrReturn(route, nameof(route));
            this.Description = description?.Trim();
            this.AppliedDirectives = directives?.Clone(this) ?? new AppliedDirectiveCollection(this);
            this.DeclaredValue = Validation.ThrowIfNullOrReturn(internalValue, nameof(internalValue));
            this.DeclaredLabel = Validation.ThrowIfNullWhiteSpaceOrReturn(declaredLabel, nameof(declaredLabel));
            this.InternalName = Validation.ThrowIfNullWhiteSpaceOrReturn(internalName, nameof(internalName));

            if (Constants.QueryLanguage.IsReservedKeyword(this.Name))
            {
                throw new GraphTypeDeclarationException($"The enum value '{this.Name}' is invalid for " +
                    $"graph type '{this.Parent.Name}'. {this.Name} is a reserved keyword.");
            }
        }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public bool IsDeprecated { get; set; }

        /// <inheritdoc />
        public string DeprecationReason { get; set; }

        /// <inheritdoc />
        public IAppliedDirectiveCollection AppliedDirectives { get; }

        /// <inheritdoc />
        public SchemaItemPath Route { get; }

        /// <inheritdoc />
        public IEnumGraphType Parent { get; }

        /// <inheritdoc />
        public object DeclaredValue { get; }

        /// <inheritdoc />
        public string DeclaredLabel { get; }

        /// <inheritdoc />
        public string InternalName { get; }
    }
}