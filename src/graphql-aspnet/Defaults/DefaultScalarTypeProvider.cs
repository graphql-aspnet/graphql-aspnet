// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A built-in, default collection of singleton instances of <see cref="IScalarGraphType"/> objects; the most fundimental unit of graphql.
    /// </summary>
    public class DefaultScalarTypeProvider : IScalarTypeProvider
    {
        private readonly IDictionary<Type, IScalarGraphType> _scalarsByType;
        private readonly IDictionary<string, IScalarGraphType> _scalarsByName;
        private readonly IDictionary<string, Type> _scalarsTypesByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultScalarTypeProvider"/> class.
        /// </summary>
        public DefaultScalarTypeProvider()
        {
            _scalarsByType = new Dictionary<Type, IScalarGraphType>();
            _scalarsByName = new Dictionary<string, IScalarGraphType>();
            _scalarsTypesByName = new Dictionary<string, Type>();

            this.AddScalar(IntScalarType.Instance);
            this.AddScalar(LongScalarType.Instance);
            this.AddScalar(UIntScalarType.Instance);
            this.AddScalar(ULongScalarType.Instance);
            this.AddScalar(FloatScalarType.Instance);
            this.AddScalar(DoubleScalarType.Instance);
            this.AddScalar(DecimalScalarType.Instance);
            this.AddScalar(BooleanScalarType.Instance);
            this.AddScalar(StringScalarType.Instance);
            this.AddScalar(DateTimeScalarType.Instance);
            this.AddScalar(DateTimeOffsetScalarType.Instance);
            this.AddScalar(ByteScalarType.Instance);
            this.AddScalar(SByteScalarType.Instance);
            this.AddScalar(GuidScalarType.Instance);
            this.AddScalar(UriScalarType.Instance);
            this.AddScalar(GraphIdScalarType.Instance);

#if NET6_0_OR_GREATER
            this.AddScalar(DateOnlyScalarType.Instance);
            this.AddScalar(TimeOnlyScalarType.Instance);
#endif
        }

        /// <summary>
        /// Determines whether the specified type is considered a leaf type in the graph system (Scalars and enumerations).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type is leaf; otherwise, <c>false</c>.</returns>
        public bool IsLeaf(Type type)
        {
            if (type == null)
                return false;

            if (type.IsEnum)
                return true;

            return _scalarsByType.ContainsKey(type);
        }

        /// <summary>
        /// Converts the given scalar reference to its formal reference type removing any
        /// nullability modifications that may be applied (e.g. converts 'int?' to 'int'). If the supplied
        /// type is already a a formal reference or is not a valid scalar type it is returned unchanged.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Type.</returns>
        public Type EnsureBuiltInTypeReference(Type type)
        {
            if (type != null && _scalarsByType.ContainsKey(type))
            {
                return _scalarsTypesByName[_scalarsByType[type].Name];
            }

            return type;
        }

        /// <summary>
        /// Determines whether the specified concrete type is a known scalar.
        /// </summary>
        /// <param name="concreteType">Type of the concrete.</param>
        /// <returns><c>true</c> if the specified concrete type is a scalar; otherwise, <c>false</c>.</returns>
        public bool IsScalar(Type concreteType)
        {
            return concreteType != null && _scalarsByType.ContainsKey(concreteType);
        }

        /// <summary>
        /// Determines whether the specified name represents a known scalar.
        /// </summary>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns><c>true</c> if the specified name is a scalar; otherwise, <c>false</c>.</returns>
        public bool IsScalar(string scalarName)
        {
            return _scalarsByName.ContainsKey(scalarName);
        }

        /// <summary>
        /// Retrieves the mapped concrete type assigned to the given scalar name.
        /// </summary>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns>Type.</returns>
        public Type RetrieveConcreteType(string scalarName)
        {
            return _scalarsTypesByName[scalarName];
        }

        /// <summary>
        /// Retrieves the scalar by its defined graph type name.
        /// </summary>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns>IScalarType.</returns>
        public IScalarGraphType RetrieveScalar(string scalarName)
        {
            return _scalarsByName[scalarName];
        }

        /// <summary>
        /// Retrieves the scalar by an assigned concrete type.
        /// </summary>
        /// <param name="concreteType">Type of the concrete.</param>
        /// <returns>IScalarType.</returns>
        public IScalarGraphType RetrieveScalar(Type concreteType)
        {
            return _scalarsByType[concreteType];
        }

        /// <summary>
        /// Adds a new scalar type to the global collection of recognized scalar values. These graph types are used as
        /// singleton instances across all schema's and all querys.
        /// </summary>
        /// <param name="graphType">The scalar type to register.</param>
        public void RegisterCustomScalar(IScalarGraphType graphType)
        {
            Validation.ThrowIfNull(graphType, nameof(graphType));
            if (string.IsNullOrWhiteSpace(graphType.Name))
            {
                throw new GraphTypeDeclarationException(
                    "The scalar must supply a name that is not null or whitespace.");
            }

            if (!GraphValidation.IsValidGraphName(graphType.Name))
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar must supply a name that that conforms to the standard rules for GraphQL. (Regex: {Constants.RegExPatterns.NameRegex})");
            }

            if (graphType.Kind != TypeKind.SCALAR)
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar's type kind must be set to '{nameof(TypeKind.SCALAR)}'.");
            }

            if (graphType.ObjectType == null)
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar must supply a value for '{nameof(graphType.ObjectType)}', is cannot be null.");
            }

            if (graphType.SourceResolver == null)
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar must supply a value for '{nameof(graphType.SourceResolver)}' that can convert data from a " +
                    $"query into the primary object type of '{graphType.ObjectType.FriendlyName()}'.");
            }

            if (graphType.ValueType == ScalarValueType.Unknown)
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar must supply a value for '{nameof(graphType.ValueType)}'. This lets the validation engine " +
                    "know what data types submitted on a user query could be parsed into a value for this scale.");
            }

            if (graphType.OtherKnownTypes == null)
            {
                throw new GraphTypeDeclarationException(
                    $"Custom scalars must supply a value for '{nameof(graphType.OtherKnownTypes)}', it cannot be null. " +
                    "Use an empty list if there are no other known types.");
            }

            var isAScalarAlready = this.IsScalar(graphType.Name);
            if (isAScalarAlready)
            {
                throw new GraphTypeDeclarationException(
                    $"A scalar named '{graphType.Name}' already exists in this graphql instance.");
            }

            isAScalarAlready = this.IsScalar(graphType.ObjectType);
            if (isAScalarAlready)
            {
                var scalar = this.RetrieveScalar(graphType.ObjectType);
                throw new GraphTypeDeclarationException(
                    $"The scalar's primary object type of '{graphType.ObjectType.FriendlyName()}' is " +
                    $"already reserved by the scalar '{scalar.Name}'. Scalar object types must be unique.");
            }

            foreach (var type in graphType.OtherKnownTypes)
            {
                isAScalarAlready = this.IsScalar(type);
                if (isAScalarAlready)
                {
                    var scalar = this.RetrieveScalar(type);
                    throw new GraphTypeDeclarationException(
                        $"The scalar's other known type of '{type.FriendlyName()}' is " +
                        $"already reserved by the scalar '{scalar.Name}'. Scalar object types must be unique.");
                }
            }

            this.AddScalar(graphType);
        }

        /// <summary>
        /// Private method to inject the scalar into the required local dictionaries.
        /// </summary>
        /// <param name="graphType">The scalar type to add.</param>
        private void AddScalar(IScalarGraphType graphType)
        {
            _scalarsByType.Add(graphType.ObjectType, graphType);
            _scalarsTypesByName.Add(graphType.Name, graphType.ObjectType);
            _scalarsByName.Add(graphType.Name, graphType);
            if (graphType.OtherKnownTypes != null && graphType.OtherKnownTypes.Count > 0)
            {
                foreach (var otherType in graphType.OtherKnownTypes)
                    _scalarsByType.Add(otherType, graphType);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IScalarGraphType> GetEnumerator()
        {
            return _scalarsByName.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator of the known concrete type classes related to the scalars known to this provider.
        /// </summary>
        /// <value>The concrete types.</value>
        public IEnumerable<Type> ConcreteTypes => _scalarsByType.Keys;
    }
}