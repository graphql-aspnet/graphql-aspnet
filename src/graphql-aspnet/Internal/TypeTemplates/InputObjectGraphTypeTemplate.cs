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
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Directives.Global;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using InputGraphFieldCollection = GraphQL.AspNet.Common.Generics.OrderedDictionary<string, GraphQL.AspNet.Interfaces.Internal.IInputGraphFieldTemplate>;

    /// <summary>
    /// An graph type template describing an INPUT_OBJECT graph type.
    /// </summary>
    public class InputObjectGraphTypeTemplate : GraphTypeTemplateBase, IInputObjectGraphTypeTemplate
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
                var constructor = objectType.GetConstructor([]);
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
                // add a specific error message for the type
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
            var name = GraphTypeNames.ParseName(this.ObjectType, TypeKind.INPUT_OBJECT);
            this.Route = new SchemaItemPath(SchemaItemPath.Join(SchemaItemCollections.Types, name));
            this.Description = this.AttributeProvider.SingleAttributeOfTypeOrDefault<DescriptionAttribute>()?.Description;

            // ------------------------------------
            // Parse the properties on this type for fields to include in the graph
            // ------------------------------------
            var parsedItems = new List<IInputGraphFieldTemplate>();

            var propMembers = this.ObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => !x.IsSpecialName && x.GetSetMethod() != null && x.GetGetMethod() != null);

            foreach (var propInfo in propMembers)
            {
                if (!this.CanBeInputField(propInfo))
                    continue;

                var parsedTemplate = new InputGraphFieldTemplate(this, propInfo);
                parsedTemplate?.Parse();
                if (parsedTemplate?.Route == null || parsedTemplate.Route.RootCollection != SchemaItemCollections.Types)
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

        /// <inheritdoc />
        protected override IEnumerable<IAppliedDirectiveTemplate> ParseAppliedDirectives()
        {
            // if the user declared their type as GraphInputUnion
            // ensure that the oneOf directive template is applied to the class
            // this is a feature of GraphInputUnion that we have to account for in business code
            // since, when using the generic version, the user has no way to supply the appropriate attributes
            // nor should they be forced to double declare.
            var foundDirectives = base.ParseAppliedDirectives()?.ToList();

            if (Validation.IsCastable<GraphInputUnion>(this.ObjectType))
            {
                // it is possible, and acceptable, that they did not add [OneOf] to their custom object inheriting from
                // GraphInputUnion. If this happens we need to make sure that the input object does apply the directive.
                foundDirectives = foundDirectives ?? [];
                if (foundDirectives.All(x => x.DirectiveType != typeof(OneOfDirective)))
                {
                    foundDirectives.Add(new AppliedDirectiveTemplate(
                        this,
                        typeof(OneOfDirective),
                        [TypeKind.INPUT_OBJECT],
                        []));
                }
            }

            return foundDirectives;
        }

        private bool CanBeInputField(PropertyInfo propInfo)
        {
            if (propInfo == null)
                return false;

            if (propInfo.HasAttribute<GraphSkipAttribute>())
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
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            base.ValidateOrThrow(validateChildren);

            if (_duplicateNames != null && _duplicateNames.Any())
            {
                throw new GraphTypeDeclarationException(
                    $"The type '{this.ObjectType.FriendlyName()}' defines multiple children with the same " +
                    $"global path key ({string.Join(",", _duplicateNames.Select(x => $"'{x}'"))}). All property paths must be unique in the " +
                    "object graph.",
                    this.ObjectType);
            }

            if (_invalidFields is { Count: > 0 })
            {
                var fieldNames = string.Join("\n", _invalidFields.Select(x => $"Field: '{x.InternalFullName} ({x.Route.RootCollection.ToString()})'"));
                throw new GraphTypeDeclarationException(
                    $"Invalid input field declaration.  The type '{this.InternalFullName}' declares fields belonging to a graph collection not allowed given its context. This type can " +
                    $"only declare the following graph collections: '{string.Join(", ", this.AllowedGraphCollectionTypes.Select(x => x.ToString()))}'. " +
                    $"If this field is declared on an object (not a controller) be sure to use '{nameof(GraphFieldAttribute)}' instead " +
                    $"of '{nameof(QueryAttribute)}' or '{nameof(MutationAttribute)}'.\n---------\n " + fieldNames,
                    this.ObjectType);
            }

            this.ValidateOneOfOrThrow();

            if (validateChildren)
            {
                foreach (var field in this.FieldTemplates.Values)
                    field.ValidateOrThrow(validateChildren);
            }
        }

        /// <summary>
        /// Validates @oneOf template requirements, if applicable and throws an exception when invalid.
        /// </summary>
        protected virtual void ValidateOneOfOrThrow()
        {
            // only applicable to @oneOf unions
            var oneOfDirectiveTemplate = this.AppliedDirectives.FirstOrDefault(x => x.DirectiveType == typeof(OneOfDirective));
            if (oneOfDirectiveTemplate is null)
                return;

            var instance = InstanceFactory.CreateInstance(this.ObjectType);
            var propGetters = InstanceFactory.CreatePropertyGetterInvokerCollection(this.ObjectType);

            if (instance is null || propGetters is null)
            {
                throw new GraphTypeDeclarationException(
                    $"Unable to validate '{this.InternalName}'. The templating engine was unable to create an instance of the input object " +
                    $"to validate default values related to @oneOf directive requirements.");
            }

            var nonNullableFields = new List<IInputGraphFieldTemplate>(_fields.Count);
            foreach (var field in this.FieldTemplates.Values)
            {
                if (field.TypeExpression.IsNonNullable)
                    nonNullableFields.Add(field);

                var value = propGetters[field.InternalName].Invoke(ref instance);
                if (value is not null)
                    nonNullableFields.Add(field);
            }

            if (nonNullableFields.Count > 0)
            {
                var fieldNames = string.Join("\n", nonNullableFields.Select(x => $"Field: '{x.InternalFullName}'"));
                throw new GraphTypeDeclarationException(
                    $"Invalid input field declaration.  The type '{this.InternalFullName}' is declared as an input union (@oneOf directive) " +
                    $"and as a requirement all fields must be nullable. The following fields are 'not nullable' by way of type expression or " +
                    $"object type declaration. \n---------\n " + fieldNames,
                    this.ObjectType);
            }
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

        private IEnumerable<SchemaItemCollections> AllowedGraphCollectionTypes => SchemaItemCollections.Types.AsEnumerable();
    }
}