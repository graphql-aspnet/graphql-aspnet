// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.TypeSystem
{
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A map of .NET types and their related built in scalar types and unions.
    /// </summary>
    public static partial class GlobalTypes
    {
        private static readonly Dictionary<Type, Type> _scalarGraphTypeTypesByConcreteType;
        private static readonly Dictionary<string, Type> _scalarsByName;
        private static readonly HashSet<Type> _fixedNamedScalars;
        private static readonly HashSet<Type> _allBuiltInScalars;

        static GlobalTypes()
        {
            _scalarGraphTypeTypesByConcreteType = new Dictionary<Type, Type>();
            _scalarsByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _fixedNamedScalars = new HashSet<Type>();
            _allBuiltInScalars = new HashSet<Type>();

            // specification defined scalars (cannot be altered)
            ValidateAndRegisterBuiltInScalar<IntScalarType>(true);
            ValidateAndRegisterBuiltInScalar<FloatScalarType>(true);
            ValidateAndRegisterBuiltInScalar<BooleanScalarType>(true);
            ValidateAndRegisterBuiltInScalar<StringScalarType>(true);
            ValidateAndRegisterBuiltInScalar<GraphIdScalarType>(true);

            // other helpful scalars added to the library for
            // convience with .NET
            ValidateAndRegisterBuiltInScalar<LongScalarType>();
            ValidateAndRegisterBuiltInScalar<UIntScalarType>();
            ValidateAndRegisterBuiltInScalar<ULongScalarType>();
            ValidateAndRegisterBuiltInScalar<DoubleScalarType>();
            ValidateAndRegisterBuiltInScalar<DecimalScalarType>();
            ValidateAndRegisterBuiltInScalar<DateTimeOffsetScalarType>();
            ValidateAndRegisterBuiltInScalar<DateTimeScalarType>();
            ValidateAndRegisterBuiltInScalar<DateOnlyScalarType>();
            ValidateAndRegisterBuiltInScalar<TimeOnlyScalarType>();
            ValidateAndRegisterBuiltInScalar<ByteScalarType>();
            ValidateAndRegisterBuiltInScalar<SByteScalarType>();
            ValidateAndRegisterBuiltInScalar<ShortScalarType>();
            ValidateAndRegisterBuiltInScalar<UShortScalarType>();
            ValidateAndRegisterBuiltInScalar<GuidScalarType>();
            ValidateAndRegisterBuiltInScalar<UriScalarType>();
        }

        private static void ValidateAndRegisterBuiltInScalar<T>(bool isFixedName = false)
            where T : IScalarGraphType
        {
            var instance = CreateScalarInstanceOrThrow(typeof(T));
            if (_scalarGraphTypeTypesByConcreteType.TryGetValue(instance.ObjectType, out Type registeredType1))
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar '{typeof(T).FriendlyName()}' is attempting to register a known type of '{instance.ObjectType.FriendlyName()}' but it is " +
                    $"already reserved by the scalar '{registeredType1.FriendlyName()}'. Built in scalar type mappings must be unique.");
            }

            if (_scalarsByName.TryGetValue(instance.Name, out Type registeredType))
            {
                throw new GraphTypeDeclarationException(
                    $"The scalar '{typeof(T).FriendlyName()}' is attempting to register with the name '{instance.Name}' but it is " +
                    $"already reserved by the scalar '{registeredType.FriendlyName()}'. Built in scalar type names must be globally unique.");
            }

            _scalarGraphTypeTypesByConcreteType.Add(instance.ObjectType, typeof(T));
            _scalarsByName.Add(instance.Name, typeof(T));

            _allBuiltInScalars.Add(typeof(T));
            if (isFixedName)
                _fixedNamedScalars.Add(typeof(T));
        }

        /// <summary>
        /// If the provided <paramref name="typeToCheck"/> represents a known, built in scalar
        /// the <see cref="Type"/> representing the associated <see cref="IScalarGraphType"/>
        /// is returned. If the <paramref name="typeToCheck"/> is not a built in scalar,
        /// <c>null</c> is returned.
        /// </summary>
        /// <remarks>
        /// e.g. if <see cref="int"/> is provided, then <see cref="IntScalarType"/>
        /// is returned.
        /// </remarks>
        /// <param name="typeToCheck">The type to check.</param>
        /// <returns>The concrete type that represents the scalar. This type
        /// is guaranteed to implement <see cref="IScalarGraphType"/>.</returns>
        public static Type FindBuiltInScalarType(Type typeToCheck)
        {
            if (typeToCheck == null)
                return null;

            if (typeToCheck.IsNullableOfT())
            {
                typeToCheck = GraphValidation.EliminateWrappersFromCoreType(
                    typeToCheck,
                    eliminateEnumerables: false,
                    eliminateTask: false,
                    eliminateNullableT: true);
            }

            if (_scalarGraphTypeTypesByConcreteType.TryGetValue(typeToCheck, out Type type))
                return type;

            if (_allBuiltInScalars.Contains(typeToCheck))
                return typeToCheck;

            return null;
        }

        /// <summary>
        /// Determines whether the provided type represents a known, globally available scalar.
        /// </summary>
        /// <param name="typeToCheck">The type to check.</param>
        /// <returns><c>true</c> if the type is a built in scalar; otherwise, <c>false</c>.</returns>
        public static bool IsBuiltInScalar(Type typeToCheck)
        {
            return FindBuiltInScalarType(typeToCheck) != null;
        }

        /// <summary>
        /// Determines whether the assigned name is the name of a known global sclar. This check is not
        /// case-sensitive.
        /// </summary>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns><c>true</c> if the name is a known global scalar; otherwise, <c>false</c>.</returns>
        internal static bool IsBuiltInScalar(string scalarName)
        {
            if (scalarName == null)
                return false;

            return _scalarsByName.ContainsKey(scalarName);
        }

        /// <summary>
        /// Determines whether the scalar matching the required name can be reformatted or re-cased to a different name.
        /// </summary>
        /// <remarks>
        /// The five specification-defined scalars (Int, Float, String, Boolean, ID) cannot be renamed and are used
        /// as part of the introspection system. All other internal scalars can be renamed or "re-cased" to match any rules
        /// for a target schema.
        /// </remarks>
        /// <param name="scalarName">Name of the scalar.</param>
        /// <returns><c>true</c> the name can be reformatted, otherwise false.</returns>
        public static bool CanBeRenamed(string scalarName)
        {
            // meh, its not a built in scalar, doesnt really matter
            if (scalarName == null)
                return true;

            // if the name represents a globally defined scalar
            // and if that scalar is declared as a fixed name
            // then don't allow it to be renamed named
            if (_scalarsByName.TryGetValue(scalarName, out Type scalarType))
            {
                return !_fixedNamedScalars.Contains(scalarType);
            }

            return true;
        }

        /// <summary>
        /// Validates that the supplied type can be used to build a scalar instance
        /// that is usable by a schema.
        /// </summary>
        /// <param name="scalarType">The type representing an <see cref="IScalarGraphType"/>.</param>
        public static void ValidateScalarTypeOrThrow(Type scalarType)
        {
            CreateAndValidateScalarType(scalarType, true);
        }

        /// <summary>
        /// Validates that the supplied scalar instance is valid and could be used by a schema
        /// instance.
        /// </summary>
        /// <param name="graphType">The graph type instance to check.</param>
        public static void ValidateScalarTypeOrThrow(IScalarGraphType graphType)
        {
            ValidateScalarType(graphType, true);
        }

        /// <summary>
        /// Validates that the supplied type can be used to build a scalar instance
        /// that is usable by a schema.
        /// </summary>
        /// <param name="scalarType">The type representing an <see cref="IScalarGraphType" />.</param>
        /// <param name="shouldThrow">if set to <c>true</c> this method will throw a <see cref="GraphTypeDeclarationException"/>
        /// if the sclar is not valid.</param>
        /// <returns>System.ValueTuple&lt;System.Boolean, IScalarGraphType&gt;.</returns>
        private static (bool IsValid, IScalarGraphType Instance) CreateAndValidateScalarType(Type scalarType, bool shouldThrow = true)
        {
            if (scalarType == null)
            {
                if (!shouldThrow)
                    return (false, null);

                throw new GraphTypeDeclarationException("~null~ is an invalid scalar type");
            }

            if (!Validation.IsCastable<IScalarGraphType>(scalarType))
            {
                if (!shouldThrow)
                    return (false, null);

                throw new GraphTypeDeclarationException(
                    $"The scalar must implement the interface '{typeof(IScalarGraphType).FriendlyName()}'.");
            }

            var paramlessConstructor = scalarType.GetConstructor(new Type[0]);
            if (paramlessConstructor == null)
            {
                if (!shouldThrow)
                    return (false, null);

                throw new GraphTypeDeclarationException(
                    "The scalar must declare a public, parameterless constructor.");
            }

            var graphType = InstanceFactory.CreateInstance(scalarType) as IScalarGraphType;
            return ValidateScalarType(graphType, shouldThrow);
        }

        /// <summary>
        /// Validates that the supplied scalar instance is valid and could be used by a schema
        /// instance.
        /// </summary>
        /// <param name="graphType">The graph type instance to check.</param>
        /// <param name="shouldThrow">if set to <c>true</c> this method will throw a <see cref="GraphTypeDeclarationException"/>
        /// if the sclar is not valid.</param>
        private static (bool IsValid, IScalarGraphType Instance) ValidateScalarType(IScalarGraphType graphType, bool shouldThrow)
        {
            if (string.IsNullOrWhiteSpace(graphType.Name))
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    "The scalar must supply a name that is not null or whitespace.");
            }

            if (!GraphValidation.IsValidGraphName(graphType.Name))
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"The scalar {graphType.GetType().FriendlyName()} must supply a name that that conforms to the standard rules for GraphQL. (Regex: {Constants.RegExPatterns.NameRegex})");
            }

            if (graphType.Kind != TypeKind.SCALAR)
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"The '{graphType.Name}' scalar's type kind must be set to '{nameof(TypeKind.SCALAR)}'.");
            }

            if (graphType.ObjectType == null)
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"The scalar '{graphType.Name}' must supply a value for '{nameof(graphType.ObjectType)}', is cannot be null.");
            }

            if (Validation.IsNullableOfT(graphType.ObjectType))
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"The scalar '{graphType.Name}' must supply the root,non-nullable type derivation for '{nameof(graphType.ObjectType)}' (e.g. 'int' not 'int?'). " +
                    $" The current value of {nameof(IScalarGraphType.ObjectType)} is a nullable type derivation.");
            }

            if (graphType.SourceResolver == null)
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"The scalar must supply a value for '{nameof(graphType.SourceResolver)}' that can convert data from a " +
                    $"query into the primary object type of '{graphType.ObjectType.FriendlyName()}'.");
            }

            if (graphType.ValueType == ScalarValueType.Unknown)
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"The scalar must supply a value for '{nameof(graphType.ValueType)}'. This lets the validation engine " +
                    "know what data types submitted on a user query could be parsed into a value for this scale.");
            }

            if (graphType.AppliedDirectives == null || graphType.AppliedDirectives.Parent != graphType)
            {
                if (!shouldThrow)
                    return (false, null);
                throw new GraphTypeDeclarationException(
                    $"Custom scalars must supply a value for '{nameof(graphType.AppliedDirectives)}', it cannot be null. " +
                    $"The '{nameof(IAppliedDirectiveCollection.Parent)}' property of the directive collection must also be set to the scalar itself.");
            }

            return (true, graphType);
        }

        /// <summary>
        /// Determines whether the provided type represents an object that is a properly constructed scalar graph type.
        /// </summary>
        /// <param name="typeToCheck">The type to check.</param>
        /// <returns><c>true</c> if the type represents a valid scalar; otherwise, <c>false</c>.</returns>
        public static bool IsValidScalarType(Type typeToCheck)
        {
            return CreateAndValidateScalarType(typeToCheck, false).IsValid;
        }

        /// <summary>
        /// Creates a new instance of the scalar. If the supplied type cannot be created
        /// as a valid <see cref="IScalarGraphType"/> an exception is thrown.
        /// </summary>
        /// <param name="scalarType">The scalar type to create.</param>
        /// <returns>IScalarGraphType.</returns>
        public static IScalarGraphType CreateScalarInstanceOrThrow(Type scalarType)
        {
            scalarType = GraphValidation.EliminateNextWrapperFromCoreType(scalarType);
            scalarType = FindBuiltInScalarType(scalarType) ?? scalarType;

            var (isValid, instance) = CreateAndValidateScalarType(scalarType, true);
            return isValid ? instance : null;
        }

        /// <summary>
        /// Creates a new instance of the scalar. If the supplied type cannot be created
        /// a valid <see cref="IScalarGraphType"/> null is returned.
        /// </summary>
        /// <param name="scalarType">The scalar type to create.</param>
        /// <returns>IScalarGraphType.</returns>
        public static IScalarGraphType CreateScalarInstance(Type scalarType)
        {
            scalarType = GraphValidation.EliminateNextWrapperFromCoreType(scalarType);
            scalarType = FindBuiltInScalarType(scalarType) ?? scalarType;

            var (isValid, instance) = CreateAndValidateScalarType(scalarType, false);
            return isValid ? instance : null;
        }

        /// <summary>
        /// Gets the list of concrete types that represent all internally defined, global scalars.
        /// </summary>
        /// <value>The set of concrete types for all the global scalars.</value>
        public static IEnumerable<Type> ScalarConcreteTypes => _scalarGraphTypeTypesByConcreteType.Keys;

        /// <summary>
        /// Gets the types that represent the <see cref="IScalarGraphType"/> objects for all internally defined, global scalars.
        /// </summary>
        /// <value>The set of scalar instance types for all global scalars.</value>
        public static IEnumerable<Type> ScalarInstanceTypes => _scalarGraphTypeTypesByConcreteType.Values;
    }
}