// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A template for harsing a C# enumeration to be used in the object graph.
    /// </summary>
    [DebuggerDisplay("Enum Template: {InternalName}")]
    public class EnumGraphTypeTemplate : BaseGraphTypeTemplate, IEnumGraphTypeTemplate
    {
        private readonly List<GraphEnumOption> _values;
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumGraphTypeTemplate"/> class.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        public EnumGraphTypeTemplate(Type enumType)
            : base(enumType)
        {
            Validation.ThrowIfNull(enumType, nameof(enumType));
            this.ObjectType = enumType;

            _values = new List<GraphEnumOption>();
        }

        /// <summary>
        /// Parses the item definition.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            if (!this.ObjectType.IsEnum)
                return;

            _name = GraphTypeNames.ParseName(this.ObjectType, TypeKind.ENUM);
            this.Description = this.ObjectType.SingleAttributeOrDefault<DescriptionAttribute>()?.Description?.Trim();
            this.Route = this.GenerateFieldPath();

            // parse the enum values for later injection
            foreach (var value in Enum.GetValues(this.ObjectType))
            {
                var fi = this.ObjectType.GetField(value.ToString());
                if (fi.SingleAttributeOrDefault<GraphSkipAttribute>() != null)
                    continue;

                var description = fi.SingleAttributeOrDefault<DescriptionAttribute>()?.Description ?? null;
                var enumAttrib = fi.SingleAttributeOrDefault<GraphEnumValueAttribute>();

                var valueName = enumAttrib?.Name?.Trim() ?? Constants.Routing.ENUM_VALUE_META_NAME;
                if (valueName.Length == 0)
                    valueName = Constants.Routing.ENUM_VALUE_META_NAME;
                valueName = valueName.Replace(Constants.Routing.ENUM_VALUE_META_NAME, value.ToString());

                var deprecated = fi.SingleAttributeOrDefault<DeprecatedAttribute>();
                _values.Add(new GraphEnumOption(this.ObjectType, valueName, description, enumAttrib != null, deprecated != null, deprecated?.Reason));
            }
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (!this.ObjectType.IsEnum)
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid Enumeration. The type '{this.ObjectType.FriendlyName()}' is not an enumeration and cannot " +
                    "be coerced to be one.");
            }

            foreach (var option in this.Values)
                option.ValidateOrThrow();
        }

        /// <summary>
        /// When overridden in a child class, this method builds the route that will be assigned to this method
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected override GraphFieldPath GenerateFieldPath()
        {
            return new GraphFieldPath(GraphFieldPath.Join(GraphCollection.Enums, _name));
        }

        /// <summary>
        /// Gets the collected set of enumeration values that this template parsed.
        /// </summary>
        /// <value>The values.</value>
        public IReadOnlyList<IEnumOption> Values => _values;

        /// <summary>
        /// Gets the fully qualified name, including namespace, of this item as it exists in the .NET code (e.g. 'Namespace.ObjectType.MethodName').
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public override string InternalFullName => this.ObjectType?.FriendlyName(true);

        /// <summary>
        /// Gets the name that defines this item within the .NET code of the application; typically a method name or property name.
        /// </summary>
        /// <value>The internal name given to this item.</value>
        public override string InternalName => this.ObjectType?.FriendlyName();

        /// <summary>
        /// Gets the security policies found via defined attributes on the item that need to be enforced.
        /// Enumerations do no enforce security policies.
        /// </summary>
        /// <value>The security policies.</value>
        public override FieldSecurityGroup SecurityPolicies => FieldSecurityGroup.Empty;

        /// <summary>
        /// Gets the kind of graph type that can be made from this template.
        /// </summary>
        /// <value>The kind.</value>
        public override TypeKind Kind => TypeKind.ENUM;
    }
}