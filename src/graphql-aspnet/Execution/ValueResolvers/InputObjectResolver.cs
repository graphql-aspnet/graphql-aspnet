// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.ValueResolvers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A resolver capable of converting a supplied JSON data into a concrete object using
    /// common property data matching rules. This resolver may be used as a base for a custom type resolver
    /// if necessary.
    /// </summary>
    [DebuggerDisplay("Input Object: {_objectType.Name}")]
    public class InputObjectResolver : BaseValueResolver
    {
        private readonly IInputObjectGraphType _graphType;
        private readonly Type _objectType;
        private readonly PropertySetterCollection _propSetters;
        private readonly Dictionary<string, IInputValueResolver> _fieldResolvers;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputObjectResolver" /> class.
        /// </summary>
        /// <param name="graphType">The graph type in the target schema for the object in question.</param>
        /// <param name="concreteType">The concrete type to render the data as.</param>
        public InputObjectResolver(IInputObjectGraphType graphType, Type concreteType)
        {
            _graphType = Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            _objectType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
            _propSetters = InstanceFactory.CreatePropertySetterInvokerCollection(concreteType);
            _fieldResolvers = new Dictionary<string, IInputValueResolver>();
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="InputObjectResolver" /> class from being created.
        /// </summary>
        /// <param name="otherResolver">The other resolver to copy core data from.</param>
        private InputObjectResolver(InputObjectResolver otherResolver)
        {
            _graphType = otherResolver._graphType;
            _objectType = otherResolver._objectType;
            _propSetters = otherResolver._propSetters;
            _fieldResolvers = otherResolver._fieldResolvers;
        }

        /// <summary>
        /// Adds a field resolver, as deteremined by input fields on a query document, that will be needed
        /// to resolve an object requested of this instance.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="resolver">The resolver.</param>
        public void AddFieldResolver(string fieldName, IInputValueResolver resolver)
        {
            _fieldResolvers.Add(fieldName, resolver);
        }

        /// <summary>
        /// Creates a copy of this value resolver injecting it with a specific collection
        /// of variables that can be used during resoltuion. Any previously wrapped variable sets
        /// should be discarded.
        /// </summary>
        /// <param name="variableData">The variable data.</param>
        /// <returns>IInputValueResolver.</returns>
        public override IInputValueResolver WithVariables(IResolvedVariableCollection variableData)
        {
            var resolver = new InputObjectResolver(this);
            resolver.VariableCollection = variableData;
            return resolver;
        }

        /// <summary>
        /// Resolves the provided query input value to the .NET object rendered by this resolver.
        /// This input value is garunteed to not be a variable reference.
        /// </summary>
        /// <param name="resolvableItem">The resolvable item.</param>
        /// <returns>System.Object.</returns>
        protected override object ResolveFromItem(IResolvableItem resolvableItem)
        {
            if (!(resolvableItem is IResolvableFieldSet fieldSEt))
                return null;

            var instance = InstanceFactory.CreateInstance(_objectType);
            foreach (var argument in fieldSEt.Fields)
            {
                var argResolver = _fieldResolvers.ContainsKey(argument.Key) ? _fieldResolvers[argument.Key] : null;

                PropertySetterInvoker propSetter = null;
                var field = _graphType.Fields.FindField(argument.Key) as ITypedItem;
                if (field != null)
                {
                    propSetter = _propSetters.ContainsKey(field.InternalName) ? _propSetters[field.InternalName] : null;
                }

                if (argResolver == null || propSetter == null)
                    continue;

                var resolvedValue = argResolver
                    .WithVariables(this.VariableCollection)
                    .Resolve(argument.Value);

                propSetter(ref instance, resolvedValue);
            }

            return instance;
        }
    }
}