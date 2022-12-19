// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Execution.Parsing.SyntaxNodes;
    using GraphQL.AspNet.Interfaces.Engine;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Internal;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A built-in, default collection of instances of <see cref="IScalarGraphType"/> objects;
    /// the most fundamental unit of graphql.
    /// </summary>
    public class DefaultScalarTypeProvider : IScalarTypeProvider
    {
        private readonly List<ScalarReference> _scalarReferences;
        private readonly IDictionary<Type, ScalarReference> _scalarsByConcreteType;
        private readonly IDictionary<string, ScalarReference> _scalarsByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultScalarTypeProvider"/> class.
        /// </summary>
        public DefaultScalarTypeProvider()
        {
            _scalarReferences = new List<ScalarReference>();
            _scalarsByConcreteType = new Dictionary<Type, ScalarReference>();
            _scalarsByName = new Dictionary<string, ScalarReference>();

            this.RegisterScalar(typeof(IntScalarType));
            this.RegisterScalar(typeof(LongScalarType));
            this.RegisterScalar(typeof(UIntScalarType));
            this.RegisterScalar(typeof(ULongScalarType));
            this.RegisterScalar(typeof(FloatScalarType));
            this.RegisterScalar(typeof(DoubleScalarType));
            this.RegisterScalar(typeof(DecimalScalarType));
            this.RegisterScalar(typeof(BooleanScalarType));
            this.RegisterScalar(typeof(StringScalarType));
            this.RegisterScalar(typeof(DateTimeScalarType));
            this.RegisterScalar(typeof(DateTimeOffsetScalarType));
            this.RegisterScalar(typeof(ByteScalarType));
            this.RegisterScalar(typeof(SByteScalarType));
            this.RegisterScalar(typeof(GuidScalarType));
            this.RegisterScalar(typeof(UriScalarType));
            this.RegisterScalar(typeof(GraphIdScalarType));
            this.RegisterScalar(typeof(ShortScalarType));
            this.RegisterScalar(typeof(UShortScalarType));

#if NET6_0_OR_GREATER
            this.RegisterScalar(typeof(DateOnlyScalarType));
            this.RegisterScalar(typeof(TimeOnlyScalarType));
#endif
        }

        /// <inheritdoc />
        public virtual bool IsLeaf(Type type)
        {
            if (type == null)
                return false;

            if (type.IsEnum)
                return true;

            return _scalarsByConcreteType.ContainsKey(type);
        }

        /// <inheritdoc />
        public virtual Type EnsureBuiltInTypeReference(Type type)
        {
            if (this.IsScalar(type))
            {
                return _scalarsByConcreteType[type].PrimaryType;
            }

            return type;
        }

        /// <inheritdoc />
        public virtual bool IsScalar(Type concreteType)
        {
            return concreteType != null && _scalarsByConcreteType.ContainsKey(concreteType);
        }

        /// <inheritdoc />
        public virtual bool IsScalar(string scalarName)
        {
            return scalarName != null && _scalarsByName.ContainsKey(scalarName);
        }

        /// <inheritdoc />
        public virtual Type RetrieveConcreteType(string scalarName)
        {
            if (this.IsScalar(scalarName))
                return _scalarsByName[scalarName].PrimaryType;
            return null;
        }

        /// <inheritdoc />
        public virtual string RetrieveScalarName(Type concreteType)
        {
            if (this.IsScalar(concreteType))
                return _scalarsByConcreteType[concreteType].Name;

            return null;
        }

        /// <inheritdoc />
        public virtual IScalarGraphType CreateScalar(string scalarName)
        {
            if (this.IsScalar(scalarName))
                return this.CreateScalarFromInstanceType(_scalarsByName[scalarName].InstanceType);

            return null;
        }

        /// <inheritdoc />
        public virtual IScalarGraphType CreateScalar(Type concreteType)
        {
            if (this.IsScalar(concreteType))
            {
                var primaryInstanceType = this.EnsureBuiltInTypeReference(concreteType);
                return this.CreateScalarFromInstanceType(_scalarsByConcreteType[primaryInstanceType].InstanceType);
            }

            return null;
        }

        /// <summary>
        /// Creates a new instance of the scalar from its formal type declaration.
        /// </summary>
        /// <param name="scalarType">Type of the scalar.</param>
        /// <returns>IScalarGraphType.</returns>
        protected virtual IScalarGraphType CreateScalarFromInstanceType(Type scalarType)
        {
            return InstanceFactory.CreateInstance(scalarType) as IScalarGraphType;
        }

        private ScalarReference FindReferenceByImplementationType(Type type)
        {
            var primaryType = this.EnsureBuiltInTypeReference(type);
            if (this.IsScalar(primaryType))
                return _scalarsByConcreteType[primaryType];

            return null;
        }

        /// <inheritdoc />
        public virtual void RegisterCustomScalar(Type scalarType)
        {
            this.RegisterScalar(scalarType);
        }

        /// <summary>
        /// Internal logic that must be excuted to register a scalar, regardless of what
        /// any subclass may do.
        /// </summary>
        /// <param name="scalarType">Type of the scalar.</param>
        private void RegisterScalar(Type scalarType)
        {
            Validation.ThrowIfNull(scalarType, nameof(scalarType));

            if (!Validation.IsCastable<IScalarGraphType>(scalarType))
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar must implement the interface '{typeof(IScalarGraphType).FriendlyName()}'.");
            }

            var paramlessConstructor = scalarType.GetConstructor(new Type[0]);
            if (paramlessConstructor == null)
            {
                throw new GraphTypeDeclarationException(
                    "The scalar must declare a public, parameterless constructor.");
            }

            var graphType = InstanceFactory.CreateInstance(scalarType) as IScalarGraphType;
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
                    $"Use '{nameof(TypeCollection)}.{nameof(TypeCollection.Empty)}' if there are no other known types.");
            }

            if (graphType.AppliedDirectives == null || graphType.AppliedDirectives.Parent != graphType)
            {
                throw new GraphTypeDeclarationException(
                    $"Custom scalars must supply a value for '{nameof(graphType.AppliedDirectives)}', it cannot be null. " +
                    $"The '{nameof(IAppliedDirectiveCollection.Parent)}' property of the directive collection must also be set to the scalar itself.");
            }

            var isAScalarAlready = this.IsScalar(graphType.Name);
            if (isAScalarAlready)
            {
                throw new GraphTypeDeclarationException(
                    $"A scalar named '{graphType.Name}' already exists in this graphql instance.");
            }

            var reference = this.FindReferenceByImplementationType(graphType.ObjectType);
            if (reference != null)
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar's primary object type of '{graphType.ObjectType.FriendlyName()}' is " +
                    $"already reserved by the scalar '{reference.Name}'. Scalar object types must be unique.");
            }

            foreach (var type in graphType.OtherKnownTypes)
            {
                var otherReference = this.FindReferenceByImplementationType(type);
                if (otherReference != null)
                {
                    throw new GraphTypeDeclarationException(
                        $"The scalar's other known type of '{type.FriendlyName()}' is " +
                        $"already reserved by the scalar '{otherReference.Name}'. Scalar object types must be unique.");
                }
            }

            var newReference = ScalarReference.Create(graphType, scalarType);
            _scalarsByConcreteType.Add(newReference.PrimaryType, newReference);
            foreach (var otherRef in newReference.OtherKnownTypes)
                _scalarsByConcreteType.Add(otherRef, newReference);

            _scalarsByName.Add(newReference.Name, newReference);
            _scalarReferences.Add(newReference);
        }

        /// <summary>
        /// Gets an enumeration of the known concrete type classes related to the scalars known to this provider.
        /// </summary>
        /// <value>The concrete types.</value>
        public IEnumerable<Type> ConcreteTypes => _scalarsByConcreteType.Keys;

        /// <inheritdoc />
        public IEnumerable<Type> ScalarInstanceTypes => _scalarReferences.Select(x => x.InstanceType);
    }
}