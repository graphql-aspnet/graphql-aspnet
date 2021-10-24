// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Common.Generics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using GraphQL.AspNet.Common.Extensions;

    /// <summary>
    /// A helper object that creates expression trees for dynamic object generation, greatly improving
    /// performance of subsequent object creation calls instead of using <see cref="Activator"/>.
    /// </summary>
    public static class InstanceFactory
    {
        private static readonly ConcurrentDictionary<Tuple<Type, Type, Type, Type>, ObjectActivator> CACHED_OBJECT_CREATORS;
        private static readonly ConcurrentDictionary<MethodInfo, MethodInvoker> CACHED_METHOD_INVOKERS;
        private static readonly ConcurrentDictionary<Type, PropertySetterCollection> CACHED_PROPERTY_INVOKERS;

        /// <summary>
        /// Initializes static members of the <see cref="InstanceFactory"/> class.
        /// </summary>
        static InstanceFactory()
        {
            CACHED_OBJECT_CREATORS = new ConcurrentDictionary<Tuple<Type, Type, Type, Type>, ObjectActivator>();
            CACHED_METHOD_INVOKERS = new ConcurrentDictionary<MethodInfo, MethodInvoker>();
            CACHED_PROPERTY_INVOKERS = new ConcurrentDictionary<Type, PropertySetterCollection>();
        }

        /// <summary>
        /// Clears all cached invokers and creators in this app instance.
        /// </summary>
        public static void Clear()
        {
            CACHED_OBJECT_CREATORS.Clear();
            CACHED_METHOD_INVOKERS.Clear();
            CACHED_PROPERTY_INVOKERS.Clear();
        }

        /// <summary>
        /// Creates a compiled method invoker that is capable of assigning a value to an object.
        /// Lambda signature: invoke(object, propValue).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>DictionaryConversionInvoker.</returns>
        public static PropertySetterCollection CreatePropertySetterInvokerCollection(Type type)
        {
            Validation.ThrowIfNull(type, nameof(type));
            if (CACHED_PROPERTY_INVOKERS.TryGetValue(type, out var collection))
                return collection;

            collection = new PropertySetterCollection();
            foreach (var propInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (propInfo.GetSetMethod() == null)
                    continue;

                // incoming object and value to assign
                var objectToAssignTo = Expression.Parameter(typeof(object).MakeByRefType(), "obj");
                var valueToAssign = Expression.Parameter(typeof(object), "val");

                // cast the object to its actual type
                // For value Types: var castObject  = (Type)objectToAssignTo;
                // For ref Types:   var castObject= objectToAssignTo as Type;
                Expression castObjectOperation;
                if (type.IsValueType)
                    castObjectOperation = Expression.Unbox(objectToAssignTo, type);
                else
                    castObjectOperation = Expression.Convert(objectToAssignTo, type);

                var castObjectVariable = Expression.Variable(type, "castedObject");
                var assignedCastObject = Expression.Assign(castObjectVariable, castObjectOperation);

                // castValue = val as Type;
                var castValueVariable = Expression.Variable(propInfo.PropertyType, "castValue");

                var castValueOperation = Expression.Convert(valueToAssign, propInfo.PropertyType);
                var assignedCastValue = Expression.Assign(castValueVariable, castValueOperation);

                // castObject.Property = castValue
                var setPropOnObject = Expression.Assign(Expression.Property(castObjectVariable, propInfo), castValueVariable);

                // recastedObject = (object)castObject
                var recastedObjectVariable = Expression.Variable(typeof(object), "recastedObject");
                var recastedObjectOperation = Expression.Convert(castObjectVariable, typeof(object));
                var copyRecast = Expression.Assign(recastedObjectVariable, recastedObjectOperation);

                // objectToAssignTo = reacastedObject
                var copyBackToInput = Expression.Assign(objectToAssignTo, recastedObjectVariable);

                var completeSetter = Expression.Block(
                    new ParameterExpression[] { castObjectVariable, castValueVariable, recastedObjectVariable }, // local vars
                    castObjectVariable,
                    assignedCastObject,
                    castValueVariable,
                    assignedCastValue,
                    setPropOnObject,
                    copyRecast,
                    copyBackToInput);

                var complete = completeSetter.ToString();
                var lambda = Expression.Lambda<PropertySetterInvoker>(completeSetter, objectToAssignTo, valueToAssign);

                var invoker = lambda.Compile();
                collection.Add(propInfo, invoker);
            }

            CACHED_PROPERTY_INVOKERS.TryAdd(type, collection);
            return collection;
        }

        /// <summary>
        /// <para>Creates a compiled lamda expression to invoke an arbitrary method via a common set of parameters. This lamda is cached for any future use greatly speeding up
        /// the invocation.</para>
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>Func&lt;System.Object, System.Object[], System.Object&gt;.</returns>
        public static MethodInvoker CreateInstanceMethodInvoker(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return null;

            if (CACHED_METHOD_INVOKERS.TryGetValue(methodInfo, out var invoker))
                return invoker;

            if (methodInfo.IsStatic)
            {
                throw new ArgumentException($"The method '{methodInfo.Name}' on type '{methodInfo.DeclaringType.FriendlyName()}' is static " +
                                            "and cannot be used to create an instance method reference.");
            }

            if (methodInfo.ReturnType == typeof(void))
            {
                throw new ArgumentException($"The method '{methodInfo.Name}' on type '{methodInfo.DeclaringType.FriendlyName()}' does not return a value. " +
                                            "This instance creator only supports methods with a return value.");
            }

            invoker = CreateMethodInvoker(methodInfo);
            CACHED_METHOD_INVOKERS.TryAdd(methodInfo, invoker);
            return invoker;
        }

        /// <summary>
        /// Creates the method invoker expression and compiles the resultant lambda.
        /// </summary>
        /// <param name="methodInfo">The method to create an expression for.</param>
        /// <returns>MethodInvoker.</returns>
        private static MethodInvoker CreateMethodInvoker(MethodInfo methodInfo)
        {
            Validation.ThrowIfNull(methodInfo, nameof(methodInfo));

            var declaredType = methodInfo.ReflectedType ?? methodInfo.DeclaringType;

            // -------------------------------------------
            // Function call parameters
            // -------------------------------------------
            // a reference to the object instance needed to call instance method (the "this").
            var objectToInvokeOn = Expression.Parameter(typeof(object).MakeByRefType(), "objectToInvokeOn");

            // create a single param of type object[] tha will hold the variable length of method arguments being passed in
            ParameterExpression inputArguments = Expression.Parameter(typeof(object[]), "args");

            // -------------------------------------------
            // method body
            // -------------------------------------------
            // cast the input object to its required Type
            var castedObjectToInvokeOn = Expression.Variable(declaredType);
            var castOperation = Expression.Assign(castedObjectToInvokeOn, Expression.Convert(objectToInvokeOn, declaredType));

            // invocation parameters to pass to the method
            ParameterInfo[] paramsInfo = methodInfo.GetParameters();

            // a set of expressions that represents
            // a casting of each supplied object to its specific type for invocation
            Expression[] argsAssignments = new Expression[paramsInfo.Length];

            // pick each arg from the supplied method parameters and create a expression
            // that casts them into the required type for the parameter position on the method
            for (var i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(inputArguments, index);
                if (paramType.IsValueType)
                    argsAssignments[i] = Expression.Unbox(paramAccessorExp, paramType);
                else
                    argsAssignments[i] = Expression.Convert(paramAccessorExp, paramType);
            }

            // a direct call to the method on the invokable object with the supplied parameters
            var methodCall = Expression.Call(castedObjectToInvokeOn, methodInfo, argsAssignments);

            // Execute the method call and assign its output to returnedVariable
            var returnedVariable = Expression.Variable(methodInfo.ReturnType);
            var methodCallResultAssigned = Expression.Assign(returnedVariable, methodCall);

            // box the method result into an object
            var boxedResult = Expression.Variable(typeof(object));
            var boxedResultAssignment = Expression.Assign(boxedResult, Expression.Convert(returnedVariable, typeof(object)));

            // assign the castedObject back to the input
            // if the passed in object was a struct then when hte method on the struct was called
            // it was operating on a different struct instance than what the caller intended
            // and we need to copy back the actual invoked instance to the reffed input object
            var reassignReffedValue = Expression.Assign(objectToInvokeOn, Expression.Convert(castedObjectToInvokeOn, typeof(object)));

            var methodBody = Expression.Block(
                new ParameterExpression[] { castedObjectToInvokeOn, returnedVariable, boxedResult },
                castOperation,
                methodCallResultAssigned,
                boxedResultAssignment,
                reassignReffedValue,
                boxedResult);

            // Create lambda expression that accepts the "this" parameter and the args from the user.
            var lambda = Expression.Lambda<MethodInvoker>(methodBody, new ParameterExpression[] { objectToInvokeOn, inputArguments });
            return lambda.Compile();
        }

        /// <summary>
        /// Creates an instance of the given type using the passed arguments as arguments to the constructor
        /// that should be invoked. Invocation calls of 3 arguments or less are compiled via an lambda expression and cached
        /// for speedier access. When able, limit your constructor parameters to 3 or less for maximum performance.
        /// </summary>
        /// <typeparam name="TType">The type of which to create an instance.</typeparam>
        /// <param name="args">The arguments to supply to a constructor declared on the object types.</param>
        /// <returns>System.Object.</returns>
        public static object CreateInstance<TType>(params object[] args)
            where TType : class
        {
            return CreateInstance(typeof(TType), args);
        }

        /// <summary>
        /// Creates an instance of the given type using the passed arguments as arguments to the constructor
        /// that should be invoked. Invocation calls of 3 arguments or less are compiled via an lambda expression and cached
        /// for speedier access. When able, limit your constructor parameters to 3 or less for maximum performance.
        /// </summary>
        /// <param name="type">The type to create.</param>
        /// <param name="args">The arguments to supply to a constructor declared the object types.</param>
        /// <returns>System.Object.</returns>
        public static object CreateInstance(Type type, params object[] args)
        {
            // Adapted from Trenki's example: https://trenki2.github.io/blog/2018/12/28/activator-createinstance-faster-alternative/ .
            // in combination with Roger Johanson: https://rogerjohansson.blog/2008/02/28/linq-expressions-creating-objects/ .
            // -----------------------------
            if (args == null)
                return CreateInstance(type);

            if ((args.Length > 3)
                || (args.Length > 0 && args[0] == null)
                || (args.Length > 1 && args[1] == null)
                || (args.Length > 2 && args[2] == null))
            {
                return Activator.CreateInstance(type, args);
            }

            // extract at most three args for key generation
            var arg0 = args.Length > 0 ? args[0] : null;
            var arg1 = args.Length > 1 ? args[1] : null;
            var arg2 = args.Length > 2 ? args[2] : null;

            // generate the key to use for caching the dynamically compiled creator function
            var key = Tuple.Create(
                type,
                arg0?.GetType() ?? typeof(TypeToIgnore),
                arg1?.GetType() ?? typeof(TypeToIgnore),
                arg2?.GetType() ?? typeof(TypeToIgnore));

            // fetch or create the function as necessary
            if (!CACHED_OBJECT_CREATORS.TryGetValue(key, out var createObject))
            {
                createObject = CreateActivator(type, key.Item2, key.Item3, key.Item4);
                CACHED_OBJECT_CREATORS.TryAdd(key, createObject);
            }

            return createObject(args);
        }

        /// <summary>
        /// Creates the lamba expression representing the "newing up" of the type given the constructor arguments supplied.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="constructorArguments">The constructor arguments.</param>
        /// <returns>ObjectActivator.</returns>
        private static ObjectActivator CreateActivator(Type objectType, params Type[] constructorArguments)
        {
            var constructorArgumentList = CreateConstructorTypeList(constructorArguments);
            var constructor = objectType.GetConstructor(constructorArgumentList.ToArray());

            if (constructor == null &&
                objectType.IsStruct() &&
                constructorArgumentList.Count == 0)
            {
                var structActivator = CreateDefaultStructActivator(objectType);
                if (structActivator != null)
                    return structActivator;
            }

            if (constructor == null)
            {
                var message = $"The type '{objectType.FriendlyName()}' does not contain a constructor that takes {constructorArgumentList.Count} parameter(s)";
                if (constructorArgumentList.Count > 0)
                {
                    message += $" of types '{string.Join(", ", constructorArgumentList.Select(x => x.FriendlyName()))}'.";
                }

                throw new InvalidOperationException(message);
            }

            ParameterInfo[] paramsInfo = constructor.GetParameters();

            // create a single param of type object[]
            ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
            Expression[] argsExp = new Expression[paramsInfo.Length];

            // pick each arg from the params array
            // and create a typed expression of them
            for (int i = 0; i < paramsInfo.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type paramType = paramsInfo[i].ParameterType;
                Expression paramAccessorExp = Expression.ArrayIndex(param, index);
                Expression paramCastExp = Expression.Convert(paramAccessorExp, paramType);
                argsExp[i] = paramCastExp;
            }

            // make a NewExpression that calls the ctor with the args we just created
            Expression newExp = Expression.New(constructor, argsExp);

            // explicit cast the created struct to object before returning
            // this will account for any structs with parameterized constructors
            // being created by the instance factory
            Expression asObjectExp = Expression.Convert(newExp, typeof(object));

            // create a lambda with the New Expression as body and our param object[] as arg
            LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), asObjectExp, param);

            // compile it
            ObjectActivator compiled = (ObjectActivator)lambda.Compile();
            return compiled;
        }

        /// <summary>
        /// Attempts to evaluate the type as a struct and try to render an expression
        /// that would call.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>ObjectActivator.</returns>
        private static ObjectActivator CreateDefaultStructActivator(Type objectType)
        {
            try
            {
                // make a NewExpression that calls the empty ctor
                ParameterExpression param = Expression.Parameter(typeof(object[]), "args");
                NewExpression newExp = Expression.New(objectType);

                // explicit cast the created struct to object
                var asObjectExp = Expression.Convert(newExp, typeof(object));

                // create a lambda with the New Expression as body and our param object[] as arg
                LambdaExpression lambda = Expression.Lambda(typeof(ObjectActivator), asObjectExp, param);

                // compile it
                return (ObjectActivator)lambda.Compile();
            }
            catch
            {
                // this could fail for a multitude of reasons (namely if our extension method
                // Type.IsStruct() is wrong Let the instance factory
                // indicate that its not a valid object (with default constructor)
                // still up in the air if this is a good idea
                return null;
            }
        }

        /// <summary>
        /// Filters the list of supplied types to those actually used for the constructor. Removes any placeholders
        /// added during key generation.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns>System.Collections.Generic.List&lt;System.Type&gt;.</returns>
        private static IReadOnlyList<Type> CreateConstructorTypeList(params Type[] types)
        {
            var constructorTypes = new List<Type>();
            if (types != null)
            {
                foreach (var type in types)
                {
                    if (type != null && type != typeof(TypeToIgnore))
                        constructorTypes.Add(type);
                }
            }

            return constructorTypes;
        }

        /// <summary>
        /// Gets the creators cached to this application instance.
        /// </summary>
        /// <value>The cached creators.</value>
        public static IReadOnlyDictionary<Tuple<Type, Type, Type, Type>,  ObjectActivator> ObjectCreators => CACHED_OBJECT_CREATORS;

        /// <summary>
        /// Gets the collection of cached method invokers for fast invocation in this app instance.
        /// </summary>
        /// <value>The method invokers.</value>
        public static IReadOnlyDictionary<MethodInfo,  MethodInvoker> MethodInvokers => CACHED_METHOD_INVOKERS;

        /// <summary>
        /// Gets the collection of cached property invokers for fast invocation in this app instance.
        /// </summary>
        /// <value>The method invokers.</value>
        public static IReadOnlyDictionary<Type,  PropertySetterCollection> PropertyInvokers => CACHED_PROPERTY_INVOKERS;
    }
}