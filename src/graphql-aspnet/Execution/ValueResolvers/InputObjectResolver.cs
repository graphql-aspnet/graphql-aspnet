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
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A resolver capable of converting a supplied JSON data into a concrete object using
    /// common property data matching rules. This resolver may be used as a base for a custom type resolver
    /// if necessary.
    /// </summary>
    [DebuggerDisplay("Input Object: {_objectType.Name}")]
    internal class InputObjectResolver : IInputValueResolver
    {
        private readonly IInputObjectGraphType _graphType;
        private readonly Type _objectType;
        private readonly PropertySetterCollection _propSetters;
        private readonly Dictionary<string, IInputValueResolver> _fieldResolvers;
        private readonly ISchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputObjectResolver" /> class.
        /// </summary>
        /// <param name="graphType">The graph type in the target schema for the object in question.</param>
        /// <param name="concreteType">The concrete type to render the data as.</param>
        /// <param name="schema">The schema that owns the supplied <paramref name="graphType"/>.</param>
        public InputObjectResolver(IInputObjectGraphType graphType, Type concreteType, ISchema schema)
        {
            _graphType = Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            _objectType = Validation.ThrowIfNullOrReturn(concreteType, nameof(concreteType));
            _propSetters = InstanceFactory.CreatePropertySetterInvokerCollection(concreteType);
            _schema = Validation.ThrowIfNullOrReturn(schema, nameof(schema));
            _fieldResolvers = new Dictionary<string, IInputValueResolver>();
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

        /// <inheritdoc />
        public object Resolve(IResolvableValueItem resolvableItem, IResolvedVariableCollection variableData = null)
        {
            if (resolvableItem is IResolvablePointer pointer)
            {
                IResolvedVariable variable = null;
                var variableFound = variableData?.TryGetValue(pointer.PointsTo, out variable) ?? false;
                if (variableFound)
                    return variable.Value;
            }

            if (resolvableItem is IResolvableNullValue)
                return null;

            var instance = InstanceFactory.CreateInstance(_objectType);
            if (resolvableItem is DefaultInputObjectResolutionValue)
                return instance;

            if (!(resolvableItem is IResolvableFieldSet suppliedFields))
            {
                SourceOrigin origin = default;
                if (resolvableItem is IDocumentPart docPart)
                {
                    origin = docPart.SourceLocation.AsOrigin();
                }

                throw new GraphExecutionException(
                    $"Unable to resolve type '{_graphType.Name}'. Expected a " +
                    "set of field values to resolve, but recieved " +
                    $"{resolvableItem?.GetType().FriendlyName()}",
                    origin);
            }

            foreach (var inputField in suppliedFields.ResolvableFields)
            {
                var resolver = _fieldResolvers.ContainsKey(inputField.Key) ? _fieldResolvers[inputField.Key] : null;

                PropertySetterInvoker propSetter = null;
                var actualField = _graphType.Fields.FindField(inputField.Key);
                if (actualField != null)
                {
                    propSetter = _propSetters.ContainsKey(actualField.InternalName) ? _propSetters[actualField.InternalName] : null;
                }

                if (resolver == null || propSetter == null)
                    continue;

                var resolvedValue = resolver.Resolve(inputField.Value, variableData);
                if (resolvedValue != null || actualField.TypeExpression.IsNullable)
                {
                    propSetter(ref instance, resolvedValue);
                }
                else if (actualField.TypeExpression.IsNonNullable)
                {
                    // if the field is not required (meaning it has a default value)
                    // the dfault value is already set by the instantiation of the
                    // input object, no set action is actually required
                    //
                    // we just need to validate that hte field does indeed
                    // have a default value (isRequired == false)
                    if (actualField.IsRequired)
                    {
                        // the document validation rules
                        // should prevent this scenario from ever happening
                        // but trap it just in case to give a helpful exception
                        SourceOrigin origin = default;
                        if (resolvableItem is IDocumentPart docPart)
                            origin = docPart.Origin;

                        throw new GraphExecutionException(
                            $"Unable to resolve type '{_graphType.Name}'. Field " +
                            $"'{actualField.Name}' received a null value but is non-nullable " +
                            $"and has no default value.",
                            origin);
                    }
                }
            }

            // check all the fields that "must have a value"
            // to ensure they are put on the input objec being constructed
            foreach (var field in _graphType.Fields.NonNullableFields)
            {
                // if a non-nullable field was supplied on the request
                // and processed successfully then skip it
                if (suppliedFields != null && suppliedFields.TryGetField(field.Name, out _))
                    continue;

                if (field.IsRequired)
                {
                    // the document validation rules
                    // should prevent this scenario from ever happening
                    // but trap it just in case to give a helpful exception
                    SourceOrigin origin = default;
                    if (resolvableItem is IDocumentPart docPart)
                    {
                        origin = docPart.SourceLocation.AsOrigin();
                    }

                    throw new GraphExecutionException(
                        $"Unable to resolve type '{_graphType.Name}'. Field " +
                        $"'{field.Name}' was not supplied but is non-nullable " +
                        $"and has no default value.",
                        origin);
                }

                var propSetter = _propSetters.ContainsKey(field.InternalName) ? _propSetters[field.InternalName] : null;
                var resolver = _fieldResolvers.ContainsKey(field.Name) ? _fieldResolvers[field.Name] : null;
                if (resolver == null || propSetter == null)
                    continue;

                var resolvedValue = resolver.Resolve(DefaultInputObjectResolutionValue.Instance);
                propSetter(ref instance, resolvedValue);
            }

            return instance;
        }

        private class DefaultInputObjectResolutionValue : IResolvableValueItem
        {
            /// <summary>
            /// Gets the single instance of this value.
            /// </summary>
            /// <value>The instance.</value>
            public static IResolvableValueItem Instance { get; } = new DefaultInputObjectResolutionValue();

            /// <summary>
            /// Prevents a default instance of the <see cref="DefaultInputObjectResolutionValue"/> class from being created.
            /// </summary>
            private DefaultInputObjectResolutionValue()
            {
            }
        }
    }
}