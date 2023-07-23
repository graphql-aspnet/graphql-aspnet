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
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Security;

    /// <summary>
    /// A base class representing common functionality shared between all field templates.
    /// </summary>
    public abstract class GraphFieldTemplateBase : SchemaItemTemplateBase, IGraphFieldTemplate
    {
        private AppliedSecurityPolicyGroup _securityPolicies;
        private GraphFieldAttribute _fieldDeclaration;
        private bool _invalidTypeExpression;
        private bool _returnsActionResult;
        private bool _duplicateUnionDeclarationDetected;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphFieldTemplateBase" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="attributeProvider">The instance that will supply the various attributes used to generate this field template.
        /// This is usually <see cref="PropertyInfo"/> or <see cref="MethodInfo"/>.</param>
        protected GraphFieldTemplateBase(IGraphTypeTemplate parent, ICustomAttributeProvider attributeProvider)
            : base(attributeProvider)
        {
            this.Parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            _securityPolicies = AppliedSecurityPolicyGroup.Empty;
            this.PossibleObjectTypes = new List<Type>();
        }

        /// <inheritdoc />
        protected override void ParseTemplateDefinition()
        {
            Type StripTasks(Type type)
            {
                return GraphValidation.EliminateWrappersFromCoreType(
                    type,
                    eliminateEnumerables: false,
                    eliminateTask: true,
                    eliminateNullableT: false);
            }

            base.ParseTemplateDefinition();

            _fieldDeclaration = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();

            // ------------------------------------
            // Build up a list of possible return types and, if applicable,
            // position the declared return type of the resolver to be at the 0th index.
            // ------------------------------------
            var potentialReturnTypes = this.GatherAllPossibleReturnedDataTypes()
                .Select(x => StripTasks(x))
                .ToList();

            // extract type info from the return type of the field
            // ensure its listed first if it needs to be
            var rootDeclaredType = StripTasks(this.DeclaredReturnType);
            if (!Validation.IsCastable<IGraphActionResult>(rootDeclaredType))
                potentialReturnTypes.Insert(0, rootDeclaredType);

            // remove duplicates and trim to only valid types
            potentialReturnTypes = potentialReturnTypes
                .Distinct()
                .Where(x => GraphValidation.IsValidGraphType(x))
                .ToList();

            // ------------------------------------
            // Extract Common Metadata
            // ------------------------------------
            this.Route = this.GenerateFieldPath();
            this.Mode = _fieldDeclaration?.ExecutionMode ?? FieldResolutionMode.PerSourceItem;
            this.Complexity = _fieldDeclaration?.Complexity;
            this.Description = this.AttributeProvider.SingleAttributeOfTypeOrDefault<DescriptionAttribute>()?.Description;
            if (_fieldDeclaration?.TypeExpression == null)
            {
                this.DeclaredTypeWrappers = null;
            }
            else
            {
                var expression = GraphTypeExpression.FromDeclaration(_fieldDeclaration.TypeExpression);
                if (!expression.IsValid)
                    _invalidTypeExpression = true;
                else
                    this.DeclaredTypeWrappers = expression.Wrappers;
            }

            // ------------------------------------
            // Build out the union definition if one was supplied
            // ------------------------------------
            this.UnionProxy = this.BuildUnionProxyInstance(potentialReturnTypes);

            // ------------------------------------
            // Calculate the correct type expression and expected return type
            // ------------------------------------
            //
            // first calculate the initial expression from what is returned by the resolver
            // based on the resolver's declarations
            var objectType = GraphValidation.EliminateWrappersFromCoreType(this.DeclaredReturnType);
            var typeExpression = GraphTypeExpression
                                    .FromType(this.DeclaredReturnType, this.DeclaredTypeWrappers)
                                    .CloneTo(Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME);

            // adjust the object type and type expression
            // if this field returns an action result
            if (Validation.IsCastable<IGraphActionResult>(objectType))
            {
                _returnsActionResult = true;
                if (this.UnionProxy != null)
                {
                    // if a union was declared preserve whatever modifer elements
                    // were declared but alter the return type to "object"
                    // (a known common element among all members of the union)
                    objectType = typeof(object);

                    // clear out the "possible types" that could be returned and
                    // limit this field to just those of the union (they are already part of the union anyways)
                    potentialReturnTypes.Clear();
                    potentialReturnTypes.AddRange(this.UnionProxy.Types);
                }
                else if (potentialReturnTypes.Count > 0)
                {
                    // the first type in the list will be the primary return type
                    // when this field is not returning a union
                    // extract its type expression AND object type to be the primary for the field
                    objectType = potentialReturnTypes[0];
                    typeExpression = GraphTypeExpression
                        .FromType(objectType, this.DeclaredTypeWrappers)
                        .CloneTo(Constants.Other.DEFAULT_TYPE_EXPRESSION_TYPE_NAME);

                    objectType = GraphValidation.EliminateWrappersFromCoreType(objectType);
                }
                else
                {
                    // ths is an error state that will be picked up during validation
                    objectType = null;
                }
            }

            this.ObjectType = objectType;
            this.TypeExpression = typeExpression;

            // done with type expression, set the final list of potential object types
            // to the core types of each return type.
            this.PossibleObjectTypes = potentialReturnTypes
                .Select(x => GraphValidation.EliminateWrappersFromCoreType(x))
                .ToList();

            // ------------------------------------
            // Async Requirements
            // ------------------------------------
            this.IsAsyncField = Validation.IsCastable<Task>(this.DeclaredReturnType);

            // ------------------------------------
            // Security Policies
            // ------------------------------------
            _securityPolicies = AppliedSecurityPolicyGroup.FromAttributeCollection(this.AttributeProvider);
        }

        /// <inheritdoc />
        public override void ValidateOrThrow()
        {
            base.ValidateOrThrow();

            if (_invalidTypeExpression)
            {
                throw new GraphTypeDeclarationException(
                    $"The field  '{this.InternalName}' defines an invalid {nameof(GraphFieldAttribute.TypeExpression)} (Value = '{_fieldDeclaration.TypeExpression}'). " +
                    $"The provided type expression must be a valid query language type expression or null.");
            }

            if (this.DeclaredReturnType == typeof(void))
            {
                throw new GraphTypeDeclarationException($"The graph field '{this.InternalName}' has a void return. All graph fields must return something.");
            }

            if (this.IsAsyncField)
            {
                // account for a mistake by the developer in using a potential return type of just "Task" instead of Task<T>
                var genericArgs = this.DeclaredReturnType.GetGenericArguments();
                if (genericArgs.Length != 1)
                {
                    throw new GraphTypeDeclarationException(
                        $"The field  '{this.InternalName}' defines a return type of'{typeof(Task).Name}' but " +
                        "defines no contained return type for the resultant model object yielding a void return after " +
                        "completion of the task. All graph methods must return a single model object. Consider using " +
                        $"'{typeof(Task<>).Name}' instead for asyncronous methods");
                }
            }

            if (_duplicateUnionDeclarationDetected)
            {
                throw new GraphTypeDeclarationException(
                        $"The field '{this.InternalName}' attempted to define a union multiple times. A union can only be " +
                        $"defined once per field. Double check the field's applied attributes.");
            }

            if (this.UnionProxy != null)
            {
                GraphValidation.EnsureGraphNameOrThrow($"{this.InternalName}[{nameof(GraphFieldAttribute)}][{nameof(IGraphUnionProxy)}]", this.UnionProxy.Name);
                if (this.UnionProxy.Types.Count < 1)
                {
                    throw new GraphTypeDeclarationException(
                        $"The field '{this.InternalName}' declares union type of '{this.UnionProxy.Name}' " +
                        "but that type includes 0 possible types in the union. Unions require 1 or more possible types. Add additional types" +
                        "or remove the union.");
                }

                // union methods MUST return a graph action result
                var unwrappedReturnType = GraphValidation.EliminateWrappersFromCoreType(this.DeclaredReturnType);
                if (!Validation.IsCastable<IGraphActionResult>(unwrappedReturnType))
                {
                    throw new GraphTypeDeclarationException(
                        $"The field '{this.InternalName}' declares union type of '{this.UnionProxy.Name}'. " +
                        $"A fields returning a union must return a {nameof(IGraphActionResult)} from the method or property resolver.");
                }
            }
            else if (this.ObjectType == null || this.ObjectType == typeof(object))
            {
                // this field is not a union but it also has not declared a proper return type.
                // this can happen if the field returns a graph action result and does not declare a return type.
                throw new GraphTypeDeclarationException(
                    $"The field '{this.InternalName}' declared no possible return types either as part of its specification or as the " +
                    "declared return type for the field. GraphQL requires the type information be known " +
                    $"to setup the schema and client tooling properly. If this field returns a '{nameof(IGraphActionResult)}' you must " +
                    "provide a graph field declaration attribute and add at least one type; be that a concrete type, an interface or a union.");
            }

            // regardless of being a union or not there must always at least one possible return type
            if (this.PossibleObjectTypes.Count == 0)
            {
                throw new GraphTypeDeclarationException(
                    $"The field '{this.InternalName}' declared no possible return types either as part of its specification or as the " +
                    "declared return type for the field. GraphQL requires the type information be known " +
                    $"to setup the schema and client tooling properly. If this field returns a '{nameof(IGraphActionResult)}' you must " +
                    "provide a graph field declaration attribute and add at least one type; be that a concrete type, an interface or a union.");
            }

            // validate each type in the list for "correctness"
            // Possible Types must conform to the rules of those required by sub-type declarations of unions and interfaces
            // interfaces: https://graphql.github.io/graphql-spec/October2021/#sec-Interfaces
            // unions: https://graphql.github.io/graphql-spec/October2021/#sec-Unions
            var enforceUnionRules = this.UnionProxy != null;
            foreach (var type in this.PossibleObjectTypes)
            {
                if (enforceUnionRules)
                {
                    if (GraphValidation.MustBeLeafType(type))
                    {
                        throw new GraphTypeDeclarationException(
                            $"The field '{this.InternalName}' declares union with a possible type of '{type.FriendlyName()}' " +
                            "but that type is a leaf value (i.e. a defined scalar or enum). Scalars and enums cannot be included in a field's possible type collection, only object types can.");
                    }

                    if (type.IsInterface)
                    {
                        throw new GraphTypeDeclarationException(
                            $"The field '{this.InternalName}'  declares union with a possible type of '{type.FriendlyName()}' " +
                            "but that type is an interface. Interfaces cannot be included in a field's possible type collection, only object types can.");
                    }
                }

                // the possible types returned by this field must never include any of the pre-defined
                // invalid types
                foreach (var invalidFieldType in Constants.InvalidFieldTemplateTypes)
                {
                    if (Validation.IsCastable(type, invalidFieldType))
                    {
                        throw new GraphTypeDeclarationException(
                            $"The field '{this.InternalName}' declares a possible return type of '{type.FriendlyName()}' " +
                            $"but that type inherits from '{invalidFieldType.FriendlyName()}' which is a reserved type declared by the graphql-aspnet library. This type cannot cannot be returned by a graphql field.");
                    }
                }

                // to ensure an object isn't arbitrarly returned as null and lost
                // ensure that the any possible type returned from this field is returnable AS the type this field declares
                // as its return type. In doing this we know that, potentially, an object returned by this
                // field "could" cast itself to the expected return type and allow field execution to continue.
                //
                // This is a helpful developer safety check, not a complete guarantee as concrete types for interface
                // declarations are not required at this stage.
                //
                // batch processed fields and those that return a IGrahpActionResult
                // are not subject to this restriction or check
                if (!_returnsActionResult && this.Mode == FieldResolutionMode.PerSourceItem && !Validation.IsCastable(type, this.ObjectType))
                {
                    throw new GraphTypeDeclarationException(
                        $"The field '{this.InternalName}' returns '{this.ObjectType.FriendlyName()}' and declares a possible type of '{type.FriendlyName()}' " +
                        $"but that type is not castable to '{this.ObjectType.FriendlyName()}' and therefore not returnable by this field. Due to the strongly-typed nature of C# any possible type on a field " +
                        "must be castable to the type of the field in order to ensure its not inadvertantly nulled out during processing. If this field returns a union " +
                        $"of multiple, disperate types consider returning '{typeof(object).Name}' from the field to ensure each possible return type can be successfully processed.");
                }
            }

            // general validation of any declaraed parameter for this field
            foreach (var argument in this.Arguments)
                argument.ValidateOrThrow();

            if (this.Complexity.HasValue && this.Complexity < 0)
            {
                throw new GraphTypeDeclarationException(
                    $"The field '{this.InternalName}' declares a complexity value of " +
                    $"`{this.Complexity.Value}`. The complexity factor must be greater than or equal to 0.");
            }

            this.ValidateBatchMethodSignatureOrThrow();
        }

        /// <summary>
        /// When overridden in a child class, this method builds the unique field path that will be assigned to this instance
        /// using the implementation rules of the concrete type.
        /// </summary>
        /// <returns>GraphRoutePath.</returns>
        protected abstract SchemaItemPath GenerateFieldPath();

        /// <summary>
        /// Type extensions used as batch methods required a speceial input and output signature for the runtime
        /// to properly supply and retrieve data from the batch. This method ensures the signature coorisponds to those requirements or
        /// throws an exception indicating the problem if one is found.
        /// </summary>
        private void ValidateBatchMethodSignatureOrThrow()
        {
            if (this.Mode != FieldResolutionMode.Batch)
                return;

            // the method MUST accept a parameter of type IEnumerable<TypeToExtend> in its signature somewhere
            // when declared in batch mode
            var requiredEnumerable = typeof(IEnumerable<>).MakeGenericType(this.SourceObjectType);
            if (this.Arguments.All(arg => arg.DeclaredArgumentType != requiredEnumerable))
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid batch method signature. The field '{this.InternalName}' declares itself as batch method but does not accept a batch " +
                    $"of data as an input parameter. This method must accept a parameter of type '{requiredEnumerable.FriendlyName()}' somewhere in its method signature to " +
                    $"be used as a batch extension for the type '{this.SourceObjectType.FriendlyName()}'.");
            }

            var declaredType = GraphValidation.EliminateWrappersFromCoreType(this.DeclaredReturnType);
            if (Validation.IsCastable<IGraphActionResult>(declaredType))
                return;

            // when a batch method doesn't return an action result, indicating the developer
            // opts to specify his return types explicitly; ensure that their chosen return type is a dictionary
            // keyed on the type being extended allowing the runtime to seperate the batch
            // for proper segmentation in the object graph.
            // --
            // when the return type is a graph action this check is deferred after results of the batch are produced
            if (!BatchResultProcessor.IsBatchDictionaryType(declaredType, this.SourceObjectType, this.ObjectType))
            {
                throw new GraphTypeDeclarationException(
                    $"Invalid batch method signature. The field '{this.InternalName}' declares a return type of '{declaredType.FriendlyName()}', however; " +
                    $"batch methods must return either an '{typeof(IGraphActionResult).FriendlyName()}' or a dictionary keyed " +
                    "on the provided source data (e.g. 'IDictionary<SourceType, ResultsPerSourceItem>').");
            }

            // ensure any possible type declared via attribution matches the value type of the resultant dictionary
            // e.g.. if they supply a union for the field but declare a dictionary of IDictionary<T,K>
            // each member of the union must be castable to 'K' in order for the runtime to properly seperate
            // and process the batch results
            var dictionaryValue = GraphValidation.EliminateWrappersFromCoreType(declaredType.GetValueTypeOfDictionary());
            foreach (var type in this.PossibleObjectTypes)
            {
                var s = dictionaryValue.FriendlyName();
                var t = type.FriendlyName();
                if (!Validation.IsCastable(type, dictionaryValue))
                {
                    throw new GraphTypeDeclarationException(
                        $"The field '{this.InternalName}' returns '{this.ObjectType.FriendlyName()}' and declares a possible type of '{type.FriendlyName()}' " +
                        $"but that type is not castable to '{this.ObjectType.FriendlyName()}' and therefore not returnable by this field. Due to the strongly-typed nature of C# any possible type on a field " +
                        "must be castable to the type of the field in order to ensure its not inadvertantly nulled out during processing. If this field returns a union " +
                        $"of multiple, disperate types consider returning '{typeof(object).Name}' from the field to ensure each possible return type can be successfully processed.");
                }
            }
        }

        /// <inheritdoc />
        public abstract IGraphFieldResolver CreateResolver();

        /// <inheritdoc />
        public abstract IGraphFieldResolverMetaData CreateResolverMetaData();

        /// <inheritdoc />
        public override IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            var list = new List<DependentType>();
            list.AddRange(base.RetrieveRequiredTypes());

            if (this.PossibleObjectTypes != null)
            {
                var dependentTypes = this.PossibleObjectTypes
                    .Select(x => new DependentType(x, GraphValidation.ResolveTypeKind(x, this.OwnerTypeKind)));
                list.AddRange(dependentTypes);
            }

            if (this.Arguments != null)
            {
                foreach (var arg in this.Arguments)
                    list.AddRange(arg.RetrieveRequiredTypes());
            }

            return list;
        }

        /// <summary>
        /// Gathers a  list, of all possible data types returned by this field. This list should be unfiltered and
        /// and contain any decorations as they are declared. Do NOT remove wrappers such as Task{T}, IEnumerable{T} or
        /// Nullable{T}.
        /// </summary>
        /// <returns>IEnumerable&lt;Type&gt;.</returns>
        protected virtual IEnumerable<Type> GatherAllPossibleReturnedDataTypes()
        {
            // extract types from [GraphField]
            var fieldAttribute = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();
            if (fieldAttribute?.Types != null)
            {
                foreach (var type in fieldAttribute.Types)
                    yield return type;
            }

            // extract types from [Union]
            var unionAttribute = this.AttributeProvider.SingleAttributeOfTypeOrDefault<UnionAttribute>();
            if (unionAttribute != null)
            {
                if (unionAttribute.UnionProxyType != null)
                    yield return unionAttribute.UnionProxyType;

                if (unionAttribute.UnionMemberTypes != null)
                {
                    foreach (var type in unionAttribute.UnionMemberTypes)
                        yield return type;
                }
            }

            // extract types from [PossibleTypes]
            var possibleTypesAttribute = this.AttributeProvider.SingleAttributeOfTypeOrDefault<PossibleTypesAttribute>();
            if (possibleTypesAttribute?.PossibleTypes != null)
            {
                foreach (var type in possibleTypesAttribute.PossibleTypes)
                    yield return type;
            }
        }

        /// <summary>
        /// Attempts to create a union proxy instance from all the attribute declarations on this field.
        /// This proxy will be used to create a union graph type for the field on a schema. If this field does
        /// not return a union this method should return <c>null</c>.
        /// </summary>
        /// <returns>IGraphUnionProxy.</returns>
        private IGraphUnionProxy BuildUnionProxyInstance(List<Type> potentialReturnTypes)
        {
            string unionTypeName = null;
            var unionDeclaredOnFieldAttribute = false;
            var unionAttributeDeclared = false;

            // extract union name from [GraphField]
            var fieldAttribute = this.AttributeProvider.SingleAttributeOfTypeOrDefault<GraphFieldAttribute>();
            if (fieldAttribute != null)
            {
                unionTypeName = fieldAttribute.UnionTypeName;
                unionDeclaredOnFieldAttribute = fieldAttribute.UnionTypeName != null;
            }

            // extract union name from [Union]
            var unionAttribute = this.AttributeProvider.SingleAttributeOfTypeOrDefault<UnionAttribute>();
            if (unionAttribute != null)
            {
                unionAttributeDeclared = true;
                unionTypeName = unionAttribute.UnionName;
            }

            // while there are multiple ways to declare a union, each field
            // can only accept 1.
            if (unionDeclaredOnFieldAttribute && unionAttributeDeclared)
            {
                _duplicateUnionDeclarationDetected = true;
                return null;
            }

            // if the only type declared is a reference to a union proxy instnatiate it and use its
            // definition for the union
            Type unionProxyType = null;
            if (potentialReturnTypes.Count == 1 && Validation.IsCastable<IGraphUnionProxy>(potentialReturnTypes[0]))
                unionProxyType = potentialReturnTypes[0];

            IGraphUnionProxy proxy = null;
            if (unionProxyType != null)
               proxy = GlobalTypes.CreateUnionProxyFromType(unionProxyType);

            // when no proxy type is declared attempt to construct the proxy from types supplied
            // if and only if a name was supplied for the union
            //
            // if it happens that two or more union proxies were declared
            // then validation will pick up the issue as a union proxy is not a valid return type
            if (proxy == null && !string.IsNullOrWhiteSpace(unionTypeName))
            {
                proxy = new GraphUnionProxy(
                    unionTypeName,
                    unionTypeName,
                    potentialReturnTypes);
            }

            return proxy;
        }

        /// <inheritdoc />
        public abstract Type DeclaredReturnType { get; }

        /// <inheritdoc />
        public abstract string DeclaredName { get; }

        /// <inheritdoc />
        public IGraphTypeTemplate Parent { get; }

        /// <inheritdoc />
        public abstract GraphFieldSource FieldSource { get; }

        /// <inheritdoc />
        public abstract TypeKind OwnerTypeKind { get; }

        /// <summary>
        /// Gets a value indicating whether returning a value from this field, as its declared in the C# code base, represents a <see cref="Task" /> that must be awaited.
        /// </summary>
        /// <value><c>true</c> if this instance is asynchronous method; otherwise, <c>false</c>.</value>
        public bool IsAsyncField { get; private set; }

        /// <inheritdoc />
        public virtual Type SourceObjectType => this.Parent?.ObjectType;

        /// <inheritdoc />
        public FieldResolutionMode Mode { get; protected set; }

        /// <inheritdoc />
        public abstract IReadOnlyList<IGraphArgumentTemplate> Arguments { get; }

        /// <inheritdoc />
        public virtual AppliedSecurityPolicyGroup SecurityPolicies => _securityPolicies;

        /// <inheritdoc />
        public GraphTypeExpression TypeExpression { get; protected set; }

        /// <inheritdoc />
        public virtual IGraphUnionProxy UnionProxy { get; protected set; }

        /// <inheritdoc />
        public MetaGraphTypes[] DeclaredTypeWrappers { get; private set; }

        /// <inheritdoc />
        public float? Complexity { get; set; }

        /// <summary>
        /// Gets the possible concrete data types that can be returned by this field.
        /// This list represents the core .NET types that will represent the various graph types. It should not include
        /// decorators such as IEnumerable{t} or Nullable{T}.
        /// </summary>
        /// <value>The potential types of data returnable by this instance.</value>
        protected List<Type> PossibleObjectTypes { get; private set; }
    }
}