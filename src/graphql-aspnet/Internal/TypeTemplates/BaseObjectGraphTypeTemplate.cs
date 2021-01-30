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
    using System.Linq;
    using System.Reflection;
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
    using GraphFieldCollection = GraphQL.AspNet.Common.Generics.OrderedDictionary<string, GraphQL.AspNet.Internal.Interfaces.IGraphTypeFieldTemplate>;

    /// <summary>
    /// A base representation of a template for an object related graph type containing common elements.
    /// </summary>
    [DebuggerDisplay("{Name} (Type: {FriendlyObjectTypeName})")]
    public abstract class BaseObjectGraphTypeTemplate : BaseGraphTypeTemplate
    {
        private readonly GraphFieldCollection _fields;
        private readonly HashSet<Type> _interfaces;
        private IEnumerable<string> _duplicateNames;
        private List<IGraphTypeFieldTemplate> _invalidFields;
        private FieldSecurityGroup _securityPolicies;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseObjectGraphTypeTemplate"/> class.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        internal BaseObjectGraphTypeTemplate(Type objectType)
            : base(objectType)
        {
            Validation.ThrowIfNull(objectType, nameof(objectType));

            _fields = new GraphFieldCollection();
            _interfaces = new HashSet<Type>();
            _securityPolicies = FieldSecurityGroup.Empty;

            // customize the error message on the thrown exception for some helpful hints.
            string rejectionReason = null;
            if (objectType.IsEnum)
                rejectionReason = $"The type '{objectType.FriendlyName()}' is an enumeration and cannot be parsed as an object graph type. Use an {typeof(IEnumGraphType).FriendlyName()} instead.";
            else if (objectType.IsValueType)
                rejectionReason = $"The type '{objectType.FriendlyName()}' is a value type and cannot be parsed as a graph type. Try using a scalar instead.";
            else if (objectType == typeof(string))
                rejectionReason = $"The type '{typeof(string).FriendlyName()}' cannot be parsed as an object graph type. Use the built in scalar instead.";
            else if (objectType.IsAbstract && objectType.IsClass)
                rejectionReason = $"The type '{objectType.FriendlyName()}' is abstract and cannot be parsed as a graph type.";

            if (rejectionReason != null)
            {
                throw new GraphTypeDeclarationException(rejectionReason, this.ObjectType);
            }

            this.ObjectType = objectType;
        }

        /// <summary>
        /// When overridden in a child class this method builds out the template according to its own individual requirements.
        /// </summary>
        protected override void ParseTemplateDefinition()
        {
            base.ParseTemplateDefinition();

            // ------------------------------------
            // Common Metadata
            // ------------------------------------
            this.Route = this.GenerateFieldPath();
            this.Description = this.SingleAttributeOfTypeOrDefault<DescriptionAttribute>()?.Description;

            // ------------------------------------
            // Security Policies
            // ------------------------------------
            _securityPolicies = FieldSecurityGroup.FromAttributeCollection(this.AttributeProvider);

            // ------------------------------------
            // Parse the methods on this type for fields to include in the graph
            // ------------------------------------
            var parsedItems = new List<IGraphTypeFieldTemplate>();

            var templateMembers = this.ObjectType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(x => !x.IsAbstract && !x.IsGenericMethod && !x.IsSpecialName).Cast<MemberInfo>()
                .Concat(this.ObjectType.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            foreach (var member in templateMembers)
            {
                if (this.CouldBeGraphField(member))
                {
                    // All members on types are "query operations" but are shown as part of the '[type]' collection
                    // kinda wierd when put against controllers which declare against [Query] and [Mutation] but are also types.
                    var parsedTemplate = this.CreateFieldTemplate(member);
                    parsedTemplate?.Parse();
                    if (parsedTemplate?.Route == null || !this.AllowedGraphCollectionTypes.Contains(parsedTemplate.Route.RootCollection))
                    {
                        _invalidFields = _invalidFields ?? new List<IGraphTypeFieldTemplate>();
                        _invalidFields.Add(parsedTemplate);
                    }
                    else
                    {
                        parsedItems.Add(parsedTemplate);
                    }
                }
            }

            // ensure no overloaded methods that are to be mapped onto the graph cause naming collisions
            // on the object graph with other methods or properties.
            _duplicateNames = parsedItems.Select(x => x.Route.Path)
                .GroupBy(x => x)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key);

            foreach (var field in parsedItems.Where(x => !_duplicateNames.Contains(x.Route.Path)))
            {
                _fields.Add(field.Route.Path, field);
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
        /// Creates the member template from the given info. If overriden in a child class methods <see cref="CreatePropertyFieldTemplate"/> and
        /// <see cref="CreateMethodFieldTemplate"/> may no longer be called. This method gives you a point of inflection to override how all
        /// field templates are created or just those for a given member type.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>GraphQL.AspNet.Internal.Interfaces.IGraphFieldTemplate.</returns>
        protected virtual IGraphTypeFieldTemplate CreateFieldTemplate(MemberInfo member)
        {
            switch (member)
            {
                case PropertyInfo pi:
                    return this.CreatePropertyFieldTemplate(pi);

                case MethodInfo mi:
                    return this.CreateMethodFieldTemplate(mi);
            }

            return null;
        }

        /// <summary>
        /// Determines whether the given container could be used as a graph field either because it is
        /// explictly declared as such or that it conformed to the required parameters of being
        /// a field.
        /// </summary>
        /// <param name="memberInfo">The member information to check.</param>
        /// <returns>
        ///   <c>true</c> if the info represents a possible graph field; otherwise, <c>false</c>.</returns>
        protected virtual bool CouldBeGraphField(MemberInfo memberInfo)
        {
            // always skip those marked as such regardless of anything else
            if (memberInfo.HasAttribute<GraphSkipAttribute>())
                return false;

            // when the member declares any known attribute in the library include it
            // and allow it to generate validation failures if its not properly constructed
            if (memberInfo.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>() != null)
                return true;

            switch (memberInfo)
            {
                case MethodInfo mi:
                    if (!GraphValidation.IsValidGraphType(mi.ReturnType, false))
                        return false;
                    if (mi.GetParameters().Any(x => !GraphValidation.IsValidGraphType(x.ParameterType, false)))
                        return false;
                    break;

                case PropertyInfo pi:
                    if (pi.GetGetMethod() == null)
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
        /// When overridden in a child class, this metyhod builds the route that will be assigned to this method
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected override GraphFieldPath GenerateFieldPath()
        {
            // a standard graph object cannot contain any route pathing or nesting like controllers can
            // before creating hte route, ensure that the declared name, by itself, is valid for graphql
            var graphName = GraphTypeNames.ParseName(this.ObjectType, TypeKind.OBJECT);
            return new GraphFieldPath(GraphFieldPath.Join(GraphCollection.Types, graphName));
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (_duplicateNames != null && _duplicateNames.Any())
            {
                throw new GraphTypeDeclarationException(
                    $"The type '{this.ObjectType.FriendlyName()}' defines multiple children with the same " +
                    $"global path key ({string.Join(",", _duplicateNames.Select(x => $"'{x}'"))}). All method and property paths must be unique in the " +
                    "object graph.",
                    this.ObjectType);
            }

            if (_invalidFields != null && _invalidFields.Count > 0)
            {
                var fieldNames = string.Join("\n", _invalidFields.Select(x => $"Field: '{x.InternalFullName} ({x.Route.RootCollection.ToString()})'"));
                throw new GraphTypeDeclarationException(
                    $"Invalid field declarations.  The type '{this.InternalFullName}' declares fields belonging to a graph collection not allowed given its context. This type can " +
                    $"only declare the following graph collections: '{string.Join(", ", this.AllowedGraphCollectionTypes.Select(x => x.ToString()))}'. " +
                    $"If this field is declared on an object (not a controller) be sure to use '{nameof(GraphFieldAttribute)}' instead " +
                    $"of '{nameof(QueryAttribute)}' or '{nameof(MutationAttribute)}'.\n---------\n " + fieldNames,
                    this.ObjectType);
            }

            foreach (var field in this.FieldTemplates.Values)
                field.ValidateOrThrow();
        }

        /// <summary>
        /// When overridden in a child, allows the class to create custom templates that inherit from <see cref="MethodGraphFieldTemplate" />
        /// to provide additional functionality or guarantee a certian type structure for all methods on this object template.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>IGraphFieldTemplate.</returns>
        protected virtual IGraphTypeFieldTemplate CreateMethodFieldTemplate(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return null;

            return new GraphTypeMethodTemplate(this, methodInfo, this.Kind);
        }

        /// <summary>
        /// When overridden in a child, allows the class to create custom templates to provide additional functionality or
        /// guarantee a certian type structure for all properties on this object template.
        /// </summary>
        /// <param name="prop">The property information.</param>
        /// <returns>IGraphFieldTemplate.</returns>
        protected virtual IGraphTypeFieldTemplate CreatePropertyFieldTemplate(PropertyInfo prop)
        {
            if (prop == null)
                return null;

            return new PropertyGraphFieldTemplate(this, prop, this.Kind);
        }

        /// <summary>
        /// Gets the security policies found via defined attributes on the item that need to be enforced.
        /// </summary>
        /// <value>The security policies.</value>
        public override FieldSecurityGroup SecurityPolicies => _securityPolicies;

        /// <summary>
        /// Gets the explicitly and implicitly decalred fields found on this instance.
        /// </summary>
        /// <value>The methods.</value>
        public IReadOnlyDictionary<string, IGraphTypeFieldTemplate> FieldTemplates => _fields;

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
        /// Gets operation types to which this object can declare a field.
        /// </summary>
        /// <value>The allowed operation types.</value>
        protected virtual HashSet<GraphCollection> AllowedGraphCollectionTypes { get; } = new HashSet<GraphCollection>() { GraphCollection.Types };

        /// <summary>
        /// Gets the collection of interfaces this type implements.
        /// </summary>
        /// <value>The interfaces.</value>
        public IEnumerable<Type> DeclaredInterfaces => _interfaces;

#if DEBUG
        /// <summary>
        /// Gets a string representing the name of the parameter's concrete type.
        /// This is an an internal helper property for helpful debugging information only.
        /// </summary>
        /// <value>The name of the parameter type friendly.</value>
        private string FriendlyObjectTypeName => this.ObjectType.FriendlyName();
#endif
    }
}