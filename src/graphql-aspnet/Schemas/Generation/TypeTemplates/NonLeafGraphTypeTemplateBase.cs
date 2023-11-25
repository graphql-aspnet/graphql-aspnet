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
    using System.Reflection;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;
    using GraphFieldCollection = GraphQL.AspNet.Common.Generics.OrderedDictionary<string, GraphQL.AspNet.Interfaces.Internal.IGraphFieldTemplate>;

    /// <summary>
    /// A base class with common functionality shared amongst templates for
    /// OBJECT, INPUT_OBJECT, INTERFACE and Controllers.
    /// </summary>
    [DebuggerDisplay("{Name} (Type: {FriendlyObjectTypeName})")]
    public abstract class NonLeafGraphTypeTemplateBase : GraphTypeTemplateBase
    {
        private readonly List<IGraphFieldTemplate> _fields;
        private readonly HashSet<Type> _interfaces;
        private List<IGraphFieldTemplate> _invalidFields;
        private AppliedSecurityPolicyGroup _securityPolicies;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonLeafGraphTypeTemplateBase"/> class.
        /// </summary>
        /// <param name="objectType">The concrete type to create a template for.</param>
        internal NonLeafGraphTypeTemplateBase(Type objectType)
            : base(objectType)
        {
            Validation.ThrowIfNull(objectType, nameof(objectType));

            _fields = new List<IGraphFieldTemplate>();
            _interfaces = new HashSet<Type>();
            _securityPolicies = AppliedSecurityPolicyGroup.Empty;

            this.AllowedSchemaItemCollections = new HashSet<SchemaItemPathCollections>()
            {
                SchemaItemPathCollections.Types,
            };

            // customize the error message on the thrown exception for some helpful hints.
            string rejectionReason = null;
            if (objectType.IsEnum)
            {
                rejectionReason = $"The type '{objectType.FriendlyName()}' is an enumeration and cannot be parsed as an {nameof(TypeKind.OBJECT)} graph type. Use an {typeof(IEnumGraphType).FriendlyName()} instead.";
            }
            else if (objectType.IsPrimitive)
            {
                rejectionReason = $"The type '{objectType.FriendlyName()}' is a primative data type and cannot be parsed as an {nameof(TypeKind.OBJECT)} graph type.";
            }
            else if (objectType == typeof(string))
            {
                rejectionReason = $"The type '{typeof(string).FriendlyName()}' cannot be parsed as an {nameof(TypeKind.OBJECT)} graph type. Use the built in scalar instead.";
            }
            else if (objectType.IsAbstract && objectType.IsClass)
            {
                rejectionReason = $"The type '{objectType.FriendlyName()}' is abstract and cannot be parsed as an {nameof(TypeKind.OBJECT)} graph type.";
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
            this.Route = this.GenerateFieldPath();
            this.Description = this.AttributeProvider.SingleAttributeOfTypeOrDefault<DescriptionAttribute>()?.Description;

            // ------------------------------------
            // Security Policies
            // ------------------------------------
            _securityPolicies = AppliedSecurityPolicyGroup.FromAttributeCollection(this.AttributeProvider);

            // ------------------------------------
            // Parse the methods on this type for fields to include in the graph
            // ------------------------------------
            _fields.Clear();

            var templateMembers = this.GatherPossibleFieldTemplates();

            foreach (var member in templateMembers)
            {
                if (this.CouldBeGraphField(member))
                {
                    var parsedTemplate = this.CreateFieldTemplate(member);
                    parsedTemplate?.Parse();

                    // ensure the schema item collection defined on the field template is consistant
                    // with this template's allowed collections
                    //
                    // OBJECT, INPUT_OBJECT and INTERFACES are restricted to [Type]
                    // but controllers are allowed to attach to the operation root collections
                    //
                    // this is used as a check against POCOs declaring [Query] fields for example
                    if (parsedTemplate?.Route == null
                        || !this.AllowedSchemaItemCollections.Contains(parsedTemplate.Route.RootCollection))
                    {
                        _invalidFields = _invalidFields ?? new List<IGraphFieldTemplate>();
                        _invalidFields.Add(parsedTemplate);
                    }
                    else
                    {
                        _fields.Add(parsedTemplate);
                    }
                }
            }

            // ------------------------------------
            // Pull a reference to any interfaces declared by this type for later inclusion in the object graph meta data.
            // ------------------------------------
            foreach (var iface in this.ObjectType.GetInterfaces().Where(x => x.IsPublic))
            {
                _interfaces.Add(iface);
            }
        }

        /// <summary>
        /// Extract the possible template members of <see cref="SchemaItemTemplateBase.ObjectType"/>
        /// that might be includable in this template.
        /// </summary>
        /// <returns>IEnumerable&lt;IFieldMemberInfoProvider&gt;.</returns>
        protected abstract IEnumerable<IMemberInfoProvider> GatherPossibleFieldTemplates();

        /// <summary>
        /// Creates the member template from the given info. If a provided <paramref name="fieldProvider"/>
        /// should not be templatized, return <c>null</c>.
        /// </summary>
        /// <param name="fieldProvider">The member to templatize.</param>
        /// <returns>GraphQL.AspNet.Internal.Interfaces.IGraphFieldTemplate.</returns>
        protected virtual IGraphFieldTemplate CreateFieldTemplate(IMemberInfoProvider fieldProvider)
        {
            if (fieldProvider?.MemberInfo == null)
                return null;

            switch (fieldProvider.MemberInfo)
            {
                case PropertyInfo pi:
                    return new PropertyGraphFieldTemplate(this, pi, fieldProvider.AttributeProvider, this.Kind);

                case MethodInfo mi:
                    return new MethodGraphFieldTemplate(this, mi, fieldProvider.AttributeProvider, this.Kind);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the given container could be used as a graph field either because it is
        /// explictly declared as such or that it conformed to the required parameters of being
        /// a field.
        /// </summary>
        /// <param name="fieldProvider">The member information to check.</param>
        /// <returns>
        ///   <c>true</c> if the info represents a possible graph field; otherwise, <c>false</c>.</returns>
        protected virtual bool CouldBeGraphField(IMemberInfoProvider fieldProvider)
        {
            // always skip those marked as such regardless of anything else
            if (fieldProvider.AttributeProvider.HasAttribute<GraphSkipAttribute>())
                return false;

            if (Constants.IgnoredFieldNames.Contains(fieldProvider.MemberInfo.Name))
                return false;

            if (fieldProvider.MemberInfo.DeclaringType?.IsRecord() ?? false)
            {
                if (Constants.IgnoredRecordFieldNames.Contains(fieldProvider.MemberInfo.Name))
                    return false;
            }

            // when the member declares any known attribute in the library include it
            // and allow it to generate validation failures if its not properly constructed
            if (fieldProvider.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>() != null)
                return true;

            // do some preliminary validation and skip those items that could never be valid
            // this is different in v2+
            switch (fieldProvider.MemberInfo)
            {
                case MethodInfo mi:
                    if (mi.IsStatic)
                        return false;
                    if (!GraphValidation.IsValidGraphType(mi.ReturnType, false))
                        return false;
                    if (mi.GetParameters().Any(x => !GraphValidation.IsValidGraphType(x.ParameterType, false)))
                        return false;
                    break;

                case PropertyInfo pi:
                    if (pi.GetGetMethod() == null)
                        return false;
                    if (pi.GetGetMethod().IsStatic)
                        return false;
                    if (pi.GetIndexParameters().Length > 0)
                        return false;

                    if (!GraphValidation.IsValidGraphType(pi.PropertyType, false))
                        return false;
                    break;
            }

            return true;
        }

        /// <summary>
        /// When overridden in a child class, this method builds the route that will be assigned to this method
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected virtual SchemaItemPath GenerateFieldPath()
        {
            // a standard graph object cannot contain any route pathing or nesting like controllers can
            // before creating hte route, ensure that the declared name, by itself, is valid for graphql
            var graphName = GraphTypeNames.ParseName(this.ObjectType, TypeKind.OBJECT);
            return new SchemaItemPath(SchemaItemPath.Join(SchemaItemPathCollections.Types, graphName));
        }

        /// <inheritdoc />
        public override void ValidateOrThrow(bool validateChildren = true)
        {
            base.ValidateOrThrow(validateChildren);

            if (_invalidFields != null && _invalidFields.Count > 0)
            {
                var fieldNames = string.Join("\n", _invalidFields.Select(x => $"Field: '{x.InternalName} ({x.Route.RootCollection.ToString()})'"));
                throw new GraphTypeDeclarationException(
                    $"Invalid field declarations.  The type '{this.InternalName}' declares fields belonging to a graph collection not allowed given its context. This type can " +
                    $"only be declared the following graph collections: '{string.Join(", ", this.AllowedSchemaItemCollections.Select(x => x.ToString()))}'. " +
                    $"If this field is declared on an object (not a controller) be sure to use '{nameof(GraphFieldAttribute)}' instead " +
                    $"of '{nameof(QueryAttribute)}' or '{nameof(MutationAttribute)}'.\n---------\n " + fieldNames,
                    this.ObjectType);
            }

            if (validateChildren)
            {
                foreach (var field in this.FieldTemplates)
                    field.ValidateOrThrow(validateChildren);
            }
        }

        /// <inheritdoc />
        public override AppliedSecurityPolicyGroup SecurityPolicies => _securityPolicies;

        /// <summary>
        /// Gets the explicitly and implicitly decalred fields found on this instance.
        /// </summary>
        /// <value>The methods.</value>
        public IReadOnlyList<IGraphFieldTemplate> FieldTemplates => _fields;

        /// <summary>
        /// Gets a set of item collections to which this object template can be declared.
        /// </summary>
        /// <value>The allowed schema item collections.</value>
        protected virtual HashSet<SchemaItemPathCollections> AllowedSchemaItemCollections { get; }

        /// <summary>
        /// Gets the declared interfaces on this item.
        /// </summary>
        /// <value>The declared interfaces.</value>
        public IEnumerable<Type> DeclaredInterfaces => _interfaces;

        /// <summary>
        /// Gets a string representing the name of the parameter's concrete type.
        /// This is an an internal helper property for helpful debugging information only.
        /// </summary>
        /// <value>The name of the parameter type friendly.</value>
        private string FriendlyObjectTypeName => this.ObjectType.FriendlyName();
    }
}