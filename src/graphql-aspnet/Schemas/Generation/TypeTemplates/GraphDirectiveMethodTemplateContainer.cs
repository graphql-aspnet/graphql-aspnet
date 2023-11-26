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
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;

    /// <summary>
    /// A collection of method templates declared on a directive template.
    /// </summary>
    public class GraphDirectiveMethodTemplateContainer
    {
        private readonly IGraphDirectiveTemplate _parent;
        private readonly Dictionary<DirectiveLocation, GraphDirectiveMethodTemplate> _templateMap;

        private HashSet<DirectiveLocation> _duplicateDirectiveLocations;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveMethodTemplateContainer" /> class.
        /// </summary>
        /// <param name="parent">The parent directive that owns this method.</param>
        public GraphDirectiveMethodTemplateContainer(IGraphDirectiveTemplate parent)
        {
            _parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            _templateMap = new Dictionary<DirectiveLocation, GraphDirectiveMethodTemplate>();
        }

        /// <inheritdoc cref="ISchemaItemTemplate.RetrieveRequiredTypes" />
        public IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            // all methods are required to be the same signatured
            // we can just pull the dependnent types on the first
            if (_templateMap.Count > 0)
                return _templateMap.Values.First().RetrieveRequiredTypes();

            return Enumerable.Empty<DependentType>();
        }

        /// <summary>
        /// Registers the method template to this container, slotting it in to handle the lifecycle hook it defines. If a hook/location
        /// combination is already registered that this method also defines, an exception will be thrown.
        /// </summary>
        /// <param name="methodTemplate">The method template.</param>
        public void RegisterMethod(GraphDirectiveMethodTemplate methodTemplate)
        {
            Validation.ThrowIfNull(methodTemplate, nameof(methodTemplate));

            var declaredLocations = methodTemplate.Locations.GetIndividualFlags<DirectiveLocation>();
            foreach (var location in declaredLocations)
            {
                if (_templateMap.ContainsKey(location))
                {
                    _duplicateDirectiveLocations = _duplicateDirectiveLocations ?? new HashSet<DirectiveLocation>();
                    _duplicateDirectiveLocations.Add(location);
                }
                else
                {
                    _templateMap.Add(location, methodTemplate);
                }

                this.Locations = this.Locations | location;
            }
        }

        /// <summary>
        /// Retrieves the resolver data mapped to the given location usage. Returns null if nothing is registered
        /// for the provided location.
        /// </summary>
        /// <param name="location">A valid graphql directive location.</param>
        /// <returns>IGraphMethod.</returns>
        public IGraphFieldResolverMetaData FindMetaData(DirectiveLocation location)
        {
            if (_templateMap.ContainsKey(location))
                return _templateMap[location].CreateResolverMetaData();

            return null;
        }

        /// <summary>
        /// Deterimines if the provided <see cref="GraphDirectiveMethodTemplate" /> has an identical signature
        /// to the method this instance represents.
        /// </summary>
        /// <param name="left">A method to check.</param>
        /// <param name="right">A method to compare against.</param>
        /// <returns><c>true</c> if the signatures are identical, <c>false</c> otherwise.</returns>
        private bool DoMethodSignaturesMatch(GraphDirectiveMethodTemplate left, GraphDirectiveMethodTemplate right)
        {
            if (left == null || right == null)
                return true;

            if (left.Arguments.Count != right.Arguments.Count)
                return false;

            for (var i = 0; i < left.Arguments.Count; i++)
            {
                if (left.Arguments[i].ObjectType != right.Arguments[i].ObjectType)
                    return false;

                if (left.Arguments[i].Name != right.Arguments[i].Name)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that this template is item and correct according to its own rules
        /// or throws an exception.
        /// </summary>
        /// <param name="validateChildren">if set to <c>true</c>
        /// any child objects this instance manages will also be checked for validity.</param>
        public virtual void ValidateOrThrow(bool validateChildren = true)
        {
            if (_duplicateDirectiveLocations != null && _duplicateDirectiveLocations.Count > 0)
            {
                var duplicatedDecs = string.Join(",", _duplicateDirectiveLocations.Select(x => $"'{x.ToString()}'"));
                throw new GraphTypeDeclarationException(
                    $"The directive '{_parent.InternalName}' attempted to register more than one method to handle " +
                    $"a single {nameof(DirectiveLocation)}. Each directive can only define, at most, one method per {nameof(DirectiveLocation)}. " +
                    $"Duplicated Locations: {duplicatedDecs}");
            }

            if (this.Count == 0)
            {
                // TODO: When warnings are added throw a warning here on a misconfigured directive
            }

            // ensure that all execution signatures match each other.
            // The specification declares that a directive is exactly one "thing"
            // with an optional set of input arguments, invocation splitting via convient overloads should not violate this.
            // Spec: https://graphql.github.io/graphql-spec/October2021/#sec-Type-System.Directives
            GraphDirectiveMethodTemplate baseExecutionMethod = null;
            foreach (var kvp in _templateMap)
            {
                kvp.Value.ValidateOrThrow(validateChildren);

                if (baseExecutionMethod == null)
                {
                    baseExecutionMethod = kvp.Value;
                    continue;
                }

                if (!this.DoMethodSignaturesMatch(baseExecutionMethod, kvp.Value))
                {
                    throw new GraphTypeDeclarationException(
                        $"The method '{kvp.Value.InternalName}' (Target Location: {kvp.Value}) declares a signature of '{kvp.Value.MethodSignature}'. " +
                        $"However, which is different than the method '{baseExecutionMethod.InternalName}'. " +
                        $"All location targeting methods on a directive must have the same method signature " +
                        $"including parameter types, names and declaration order.");
                }
            }
        }

        /// <summary>
        /// Creates the a dictionary of metadata items, keyed by the location where each resolver
        /// should execute..
        /// </summary>
        /// <returns>IReadOnlyDictionary&lt;DirectiveLocation, IGraphFieldResolverMetaData&gt;.</returns>
        public IReadOnlyDictionary<DirectiveLocation, IGraphFieldResolverMetaData> CreateMetadataCollection()
        {
            var dicOut = new Dictionary<DirectiveLocation, IGraphFieldResolverMetaData>();
            foreach (var kvp in _templateMap)
            {
                dicOut.Add(kvp.Key, kvp.Value.CreateResolverMetaData());
            }

            return dicOut;
        }

        /// <summary>
        /// Gets the total number of registrations tracked by this instance.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _templateMap.Count;

        /// <summary>
        /// Gets the argument collection this directive requires for invocation.
        /// </summary>
        /// <value>The arguments.</value>
        public IEnumerable<IGraphArgumentTemplate> Arguments
        {
            get
            {
                var args = _templateMap.Values.FirstOrDefault()?.Arguments;
                return args ?? Enumerable.Empty<IGraphArgumentTemplate>();
            }
        }

        /// <summary>
        /// Gets the locations declared amongst the methods in this collection.
        /// </summary>
        /// <value>The locations.</value>
        public DirectiveLocation Locations { get; private set; }
    }
}