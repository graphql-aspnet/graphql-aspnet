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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// A collection of known methods that can be invoked by the run time
    /// in response to the directive being included in a query document at various locations.
    /// </summary>
    public class GraphDirectiveMethodTemplateContainer : IEnumerable<GraphDirectiveMethodTemplate>
    {
        private readonly IGraphDirectiveTemplate _parent;
        private readonly Dictionary<DirectiveLifeCycle, GraphDirectiveMethodTemplate> _templateMap;
        private GraphDirectiveMethodTemplate _masterMethod;

        private HashSet<DirectiveLifeCycle> _duplciateDeclarations;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphDirectiveMethodTemplateContainer" /> class.
        /// </summary>
        /// <param name="parent">The parent directive that owns this method.</param>
        public GraphDirectiveMethodTemplateContainer(IGraphDirectiveTemplate parent)
        {
            _parent = Validation.ThrowIfNullOrReturn(parent, nameof(parent));
            _templateMap = new Dictionary<DirectiveLifeCycle, GraphDirectiveMethodTemplate>();
        }

        /// <summary>
        /// Retrieves the concrete types that this instance may return in response to a field request.
        /// </summary>
        /// <returns>IEnumerable&lt;Type&gt;.</returns>
        public IEnumerable<DependentType> RetrieveRequiredTypes()
        {
            if (_masterMethod == null)
                return Enumerable.Empty<DependentType>();

            // a method is dependent on those types required by all fields
            // as well as an input types needed for its method arguments
            return _masterMethod.Arguments.SelectMany(arg => arg.RetrieveRequiredTypes());
        }

        /// <summary>
        /// Registers the method template to this container, slotting it in to handle the lifecycle hook it defines. If a hook/location
        /// combination is already registered that this method also defines, an exception will be thrown.
        /// </summary>
        /// <param name="methodTemplate">The method template.</param>
        public void RegisterMethod(GraphDirectiveMethodTemplate methodTemplate)
        {
            Validation.ThrowIfNull(methodTemplate, nameof(methodTemplate));

            if (_templateMap.ContainsKey(methodTemplate.LifeCycle))
            {
                _duplciateDeclarations = _duplciateDeclarations ?? new HashSet<DirectiveLifeCycle>();
                _duplciateDeclarations.Add(methodTemplate.LifeCycle);
            }
            else
            {
                _templateMap.Add(methodTemplate.LifeCycle, methodTemplate);
            }

            _masterMethod = _masterMethod ?? methodTemplate;
        }

        /// <summary>
        /// Retrieves the method template mapped to the given lifecycle and location. Returns null if nothing is registered.
        /// </summary>
        /// <param name="lifeCycle">The life cycle hook.</param>
        /// <returns>IGraphMethod.</returns>
        public IGraphMethod FindMethod(DirectiveLifeCycle lifeCycle)
        {
            return _templateMap.ContainsKey(lifeCycle) ? _templateMap[lifeCycle] : null;
        }

        /// <summary>
        /// Deterimines if the provided <see cref="GraphDirectiveMethodTemplate" /> has an identical signature
        /// to the method this instance represents.
        /// </summary>
        /// <param name="methodToCheck">The method to check.</param>
        /// <returns><c>true</c> if the signatures are identical, <c>false</c> otherwise.</returns>
        private bool MatchesSignature(GraphDirectiveMethodTemplate methodToCheck)
        {
            if (_masterMethod == null || methodToCheck == _masterMethod)
                return true;

            if (methodToCheck.Arguments.Count != _masterMethod.Arguments.Count)
                return false;

            for (var i = 0; i < methodToCheck.Arguments.Count; i++)
            {
                if (methodToCheck.Arguments[i].ObjectType != _masterMethod.Arguments[i].ObjectType)
                    return false;

                if (methodToCheck.Arguments[i].Name != _masterMethod.Arguments[i].Name)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// When overridden in a child class, allows the template to perform some final validation checks
        /// on the integrity of itself. An exception should be thrown to stop the template from being
        /// persisted if the object is unusable or otherwise invalid in the manner its been built.
        /// </summary>
        public void ValidateOrThrow()
        {
            if (_duplciateDeclarations != null && _duplciateDeclarations.Count > 0)
            {
                var duplicatedDecs = string.Join(",", _duplciateDeclarations.Select(x => x.ToString()));
                throw new GraphTypeDeclarationException(
                    $"The directive '{_parent.InternalFullName}' attempted to register more than one method for a single lifecycle. Each directive can only define, at most, one method per {nameof(DirectiveLifeCycle)}. " +
                    $"Duplicated Lifecycle methods: {duplicatedDecs}");
            }

            if (this.Count == 0)
            {
                throw new GraphTypeDeclarationException(
                    $"The directive '{_parent.InternalFullName}' declared no actionable methods to invoke. Either mark this directive with '{nameof(GraphSkipAttribute)}' " +
                    $"or declare at least one valid lifecycle method.");
            }

            // ensure that all signatures match each other, the specification declares that a directive is exactly one "thing"
            // with an optional set of input arguments, invocation splitting via convient overloads should not violate this.
            // spec: https://graphql.github.io/graphql-spec/June2018/#sec-Type-System.Directives
            foreach (var method in this)
            {
                method.ValidateOrThrow();
                if (!this.MatchesSignature(method))
                {
                    throw new GraphTypeDeclarationException(
                        "All methods of a directive MUST be declared with the same method signature, including parameter names (not just types), to maintain consistancy across the " +
                        $"object graph. The method '{_masterMethod.InternalFullName}' declares a signature of '{_masterMethod.MethodSignature}' but the " +
                        $"method '{method.InternalFullName}' declares a signature of '{method.MethodSignature}.");
                }
            }
        }

        /// <summary>
        /// Gets the total number of registrations tracked by this instance.
        /// </summary>
        /// <value>The count.</value>
        public int Count => _templateMap.Count;

        /// <summary>
        /// Gets the argument collection this directive contains.
        /// </summary>
        /// <value>The arguments.</value>
        public IEnumerable<IGraphFieldArgumentTemplate> Arguments => _masterMethod?.Arguments ?? Enumerable.Empty<IGraphFieldArgumentTemplate>();

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<GraphDirectiveMethodTemplate> GetEnumerator()
        {
            return _templateMap.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}