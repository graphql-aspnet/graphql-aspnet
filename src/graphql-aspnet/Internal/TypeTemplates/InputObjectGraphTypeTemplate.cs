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
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using InputGraphFieldCollection = GraphQL.AspNet.Common.Generics.OrderedDictionary<string, GraphQL.AspNet.Internal.Interfaces.IInputGraphFieldTemplate>;

    /// <summary>
    /// A representation of the meta data of any given class that could be represented
    /// as an input object graph type in an <see cref="ISchema"/>.
    /// </summary>
    public class InputObjectGraphTypeTemplate : BaseGraphTypeTemplate, IInputObjectGraphTypeTemplate
    {
        private IEnumerable<string> _duplicateNames;
        private List<IInputGraphFieldTemplate> _invalidFields;
        private InputGraphFieldCollection _fields;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputObjectGraphTypeTemplate"/> class.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        public InputObjectGraphTypeTemplate(Type objectType)
            : base(objectType)
        {
            _fields = new InputGraphFieldCollection();

            // customize the error message on the thrown exception for some helpful hints.
            string rejectionReason = null;
            if (objectType.IsEnum)
            {
                rejectionReason = $"The type '{objectType.FriendlyName()}' is an enumeration and cannot be parsed as an {nameof(TypeKind.INPUT_OBJECT)} graph type. Use an {typeof(IEnumGraphType).FriendlyName()} instead.";
            }
            else if (GraphQLProviders.ScalarProvider.IsScalar(objectType))
            {
                rejectionReason = $"The type '{objectType.FriendlyName()}' is a registered {nameof(TypeKind.SCALAR)} and cannot be parsed as an {nameof(TypeKind.INPUT_OBJECT)} graph type. Try using the scalar definition instead.";
            }
            else if (objectType == typeof(string))
            {
                rejectionReason = $"The type '{typeof(string).FriendlyName()}' cannot be parsed as an {nameof(TypeKind.INPUT_OBJECT)} graph type. Use the built in scalar instead.";
            }
            else if (objectType.IsAbstract && objectType.IsClass)
            {
                rejectionReason = $"The type '{objectType.FriendlyName()}' is abstract and cannot be parsed as an {nameof(TypeKind.INPUT_OBJECT)} graph type.";
            }
            else if (objectType.IsClass)
            {
                // class objects MUST declare a default constructor
                // so it can be used in a 'new T()' operation when generating
                // input params
                var constructor = objectType.GetConstructor(new Type[0]);
                if (constructor == null || !constructor.IsPublic)
                {
                    rejectionReason =
                        $"The type '{objectType.FriendlyName()}' does not declare a public, parameterless constructor " +
                        $"and cannot be used as an {nameof(TypeKind.INPUT_OBJECT)} graph type.";
                }
            }
            else if (objectType.IsInterface)
            {
                rejectionReason =
                    $"The type '{objectType.FriendlyName()}' is an interface and cannot be used as an {nameof(TypeKind.INPUT_OBJECT)} graph type.";
            }
            else if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                // since KeyValuePair<,> is pretty common
                // and a specific error message for the type
                rejectionReason =
                    $"The type '{objectType.FriendlyName()}' cannot be used as an {nameof(TypeKind.INPUT_OBJECT)} graph type. '{typeof(KeyValuePair<,>).FriendlyName()}' does not " +
                    $"declare public setters for its Key and Value properties.";
            }

            if (rejectionReason != null)
            {
                throw new GraphTypeDeclarationException(rejectionReason, this.ObjectType);
            }

            this.ObjectType = objectType;
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            // ------------------------------------
            // Common Metadata
            // ------------------------------------
            this.Route = new SchemaItemPath(SchemaItemPath.Join(
                GraphCollection.Types,
                GraphTypeNames.ParseName(this.ObjectType, TypeKind.INPUT_OBJECT)));
            this.Description = this.AttributeProvider.SingleAttributeOfTypeOrDefault<DescriptionAttribute>()?.Description;

            // ------------------------------------
            // Parse the properties on this type for fields to include in the graph
            // ------------------------------------
            var parsedItems = new List<IInputGraphFieldTemplate>();

            var propMembers = this.ObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.IsSpecialName
                    && x.GetSetMethod() != null && x.GetGetMethod() != null);

            foreach (var propInfo in propMembers)
            {
                if (!this.CanBeInputField(propInfo))
                    continue;

                var parsedTemplate = new InputGraphFieldTemplate(this, propInfo);
                parsedTemplate?.Parse();
                if (parsedTemplate?.Route == null || parsedTemplate.Route.RootCollection != GraphCollection.Types)
                {
                    _invalidFields = _invalidFields ?? new List<IInputGraphFieldTemplate>();
                    _invalidFields.Add(parsedTemplate);
                }
                else
                {
                    parsedItems.Add(parsedTemplate);
                }
            }

            // ensure no duplicates are possible
            _duplicateNames = parsedItems.Select(x => x.Route.Path)
                .GroupBy(x => x)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key);

            foreach (var field in parsedItems.Where(x => !_duplicateNames.Contains(x.Route.Path)))
            {
                _fields.Add(field.Route.Path, field);
            }
        }

        private bool CanBeInputField(PropertyInfo propInfo)
        {
            if (propInfo == null)
                return false;

            if (propInfo.Attributes.SingleAttributeOrDefault<GraphSkipAttribute>() != null)
                return false;

            if (Constants.IgnoredFieldNames.Contains(propInfo.Name))
                return false;

            var type = propInfo.PropertyType;
            if (Validation.IsCastable<Task>(type))
                return false;

            var objType = GraphValidation.EliminateWrappersFromCoreType(propInfo.PropertyType);
            if (objType.IsInterface || Validation.IsCastable<IGraphUnionProxy>(objType) || Validation.IsCastable<IGraphActionResult>(objType))
                return false;

            return true;
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (_duplicateNames != null && _duplicateNames.Any())
            {
                throw new GraphTypeDeclarationException(
                    $"The type '{this.ObjectType.FriendlyName()}' defines multiple children with the same " +
                    $"global path key ({string.Join(",", _duplicateNames.Select(x => $"'{x}'"))}). All property paths must be unique in the " +
                    "object graph.",
                    this.ObjectType);
            }

            if (_invalidFields != null && _invalidFields.Count > 0)
            {
                var fieldNames = string.Join("\n", _invalidFields.Select(x => $"Field: '{x.InternalFullName} ({x.Route.RootCollection.ToString()})'"));
                throw new GraphTypeDeclarationException(
                    $"Invalid input field declaration.  The type '{this.InternalFullName}' declares fields belonging to a graph collection not allowed given its context. This type can " +
                    $"only declare the following graph collections: '{string.Join(", ", this.AllowedGraphCollectionTypes.Select(x => x.ToString()))}'. " +
                    $"If this field is declared on an object (not a controller) be sure to use '{nameof(GraphFieldAttribute)}' instead " +
                    $"of '{nameof(QueryAttribute)}' or '{nameof(MutationAttribute)}'.\n---------\n " + fieldNames,
                    this.ObjectType);
            }

            foreach (var field in this.FieldTemplates.Values)
                field.ValidateOrThrow();
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<string, IInputGraphFieldTemplate> FieldTemplates => _fields;

        /// <inheritdoc />
        public override TypeKind Kind => TypeKind.INPUT_OBJECT;

        /// <inheritdoc />
        public override AppliedSecurityPolicyGroup SecurityPolicies => AppliedSecurityPolicyGroup.Empty;

        /// <inheritdoc />
        public override string InternalFullName => this.ObjectType?.FriendlyName(true);

        /// <inheritdoc />
        public override string InternalName => this.ObjectType?.FriendlyName();

        private IEnumerable<GraphCollection> AllowedGraphCollectionTypes => GraphCollection.Types.AsEnumerable();
    }
}