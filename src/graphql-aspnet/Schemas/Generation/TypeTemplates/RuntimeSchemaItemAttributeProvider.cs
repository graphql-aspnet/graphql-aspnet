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
    using System.Reflection;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Interfaces.Configuration.Templates;

    /// <summary>
    /// A custom <see cref="ICustomAttributeProvider"/> that can provide runtime declared attributes
    /// to the templating engine, instead of them being provided at design time on a method.
    /// </summary>
    public class RuntimeSchemaItemAttributeProvider : ICustomAttributeProvider
    {
        private readonly IGraphQLResolvableSchemaItemDefinition _fieldDef;
        private readonly object[] _onlyDefinedAttributes;
        private readonly object[] _allAttributes;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuntimeSchemaItemAttributeProvider"/> class.
        /// </summary>
        /// <param name="fieldDefinition">The field definition created at runtime
        /// that contains the attributes to be provided.</param>
        public RuntimeSchemaItemAttributeProvider(IGraphQLResolvableSchemaItemDefinition fieldDefinition)
        {
            _fieldDef = Validation.ThrowIfNullOrReturn(fieldDefinition, nameof(fieldDefinition));

            var all = new List<object>();
            var onlyDefined = new List<object>();

            all.AddRange(_fieldDef.Attributes);
            all.AddRange(_fieldDef.Resolver.Method.GetCustomAttributes(true));

            onlyDefined.AddRange(_fieldDef.Attributes);

            _onlyDefinedAttributes = onlyDefined.ToArray();
            _allAttributes = all.ToArray();
        }

        /// <inheritdoc />
        public object[] GetCustomAttributes(bool inherit)
        {
            return inherit ? _allAttributes : _onlyDefinedAttributes;
        }

        /// <inheritdoc />
        public object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            var attribs = this.GetCustomAttributes(inherit);

            var outList = new List<object>();
            foreach (var attrib in attribs)
            {
                if (Validation.IsCastable(attrib?.GetType(), attributeType))
                    outList.Add(attrib);
            }

            return outList.ToArray();
        }

        /// <inheritdoc />
        public bool IsDefined(Type attributeType, bool inherit)
        {
            var attribs = this.GetCustomAttributes(inherit);
            foreach (var attrib in attribs)
            {
                if (attrib?.GetType() == attributeType)
                    return true;
            }

            return false;
        }
    }
}