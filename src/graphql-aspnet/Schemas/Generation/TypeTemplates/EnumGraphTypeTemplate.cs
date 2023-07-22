// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// An graph type template describing an ENUM graph type.
    /// </summary>
    [DebuggerDisplay("Enum Template: {InternalName}")]
    public class EnumGraphTypeTemplate : GraphTypeTemplateBase, IEnumGraphTypeTemplate
    {
        private readonly List<EnumValueTemplate> _values;
        private Dictionary<string, IList<string>> _valuesTolabels;
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumGraphTypeTemplate"/> class.
        /// </summary>
        /// <param name="enumType">The enum being templated.</param>
        public EnumGraphTypeTemplate(Type enumType)
            : base(enumType)
        {
            Validation.ThrowIfNull(enumType, nameof(enumType));
            this.ObjectType = enumType;

            _values = new List<EnumValueTemplate>();
            _valuesTolabels = new Dictionary<string, IList<string>>();
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            if (!this.ObjectType.IsEnum)
                return;

            base.ParseTemplateDefinition();

            _name = GraphTypeNames.ParseName(this.ObjectType, TypeKind.ENUM);
            this.Description = this.ObjectType.SingleAttributeOrDefault<DescriptionAttribute>()?.Description?.Trim();
            this.Route = new SchemaItemPath(SchemaItemPath.Join(SchemaItemCollections.Enums, _name));

            // parse the enum values for later injection
            var labels = Enum.GetNames(this.ObjectType);
            foreach (var label in labels)
            {
                var fi = this.ObjectType.GetField(label);

                if (fi.SingleAttributeOrDefault<GraphSkipAttribute>() != null)
                    continue;

                var optionTemplate = new EnumValueTemplate(this, fi);
                optionTemplate.Parse();

                // we must have unique labels to values
                // (no enums with duplicate values)
                // to prevent potential ambiguity in down stream
                // requests
                //
                // for instance, if an enum existed such that:
                // enum SomeEnum
                // {
                //    Value1 = 1,
                //    Value2 = 1,
                // }
                //
                // then the operation
                // var enumValue = (SomeEnum)1;
                //
                // is indeterminate as to which is the appropriate
                // label to apply to the value for conversion
                // into the coorisponding enum graph type
                // and ultimately serialization
                // to a requestor
                //
                // keep track of the number of labels share a valid so we can
                // throw an exception during validation if there are any duplicates
                if (!_valuesTolabels.ContainsKey(optionTemplate.NumericValueAsString))
                    _valuesTolabels.Add(optionTemplate.NumericValueAsString, new List<string>());

                _valuesTolabels[optionTemplate.NumericValueAsString].Add(label);
                if (_valuesTolabels[optionTemplate.NumericValueAsString].Count != 1)
                    continue;

                _values.Add(optionTemplate);
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
                    "be coerced to be one.",
                    this.ObjectType);
            }

            var duplicatedValues = _valuesTolabels.Where(x => x.Value.Count > 1).ToList();
            if (duplicatedValues.Count > 0)
            {
                var builder = new StringBuilder();
                for (var i = 0; i < duplicatedValues.Count; i++)
                {
                    var dupKVP = duplicatedValues[i];
                    builder.Append($"{{{string.Join(", ", dupKVP.Value)}}} == {dupKVP.Key}");
                    if (i < duplicatedValues.Count - 1)
                        builder.Append(" || ");
                }

                var msg = $"Invalid Enumeration. The type '{this.ObjectType.FriendlyName()}' is indeterminate and cannot " +
                    "be used as a graph type. Ensure all enum labels have unique values.  " +
                    $"({builder})";

                throw new GraphTypeDeclarationException(msg, this.ObjectType);
            }

            foreach (var option in this.Values)
                option.ValidateOrThrow();

            _valuesTolabels = null;
        }

        /// <inheritdoc />
        public IReadOnlyList<IEnumValueTemplate> Values => _values;

        /// <inheritdoc />
        public override AppliedSecurityPolicyGroup SecurityPolicies => AppliedSecurityPolicyGroup.Empty;

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.ENUM;
    }
}