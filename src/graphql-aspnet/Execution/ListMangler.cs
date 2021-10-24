// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Internal;

    /* Motivation
    *  ------------------------------
    *  Internally, this library maintains all its array structures as List<T>
    *  However, the developer may wish to accept an list of objects for an argument
    *  as an array (i.e. T[]).  This class performs the necessary restructuring
    *  of turning List<T> into T[]  as necessary to ensure a match for the
    *  target.
    */

    /// <summary>
    /// <para>
    /// Converts a list structure from one format to another.
    /// </para>
    /// <para>
    /// i.e. Converts <c>List{List{T}}</c> to <c>T[][]</c>, <c>IEnumerable{T[]}</c>  etc.
    /// </para>
    ///
    /// </summary>
    public class ListMangler
    {
        private static ConcurrentDictionary<Type, ListMap> _allMaps = new ConcurrentDictionary<Type, ListMap>();

        private static ListMap FindOrCreateMap(Type targetType)
        {
            if (_allMaps.TryGetValue(targetType, out var foundMap))
                return foundMap;

            var targets = new List<ElementTarget>();
            var stripped = targetType;
            while (GraphValidation.IsValidListType(stripped))
            {
                if (stripped.IsArray)
                    targets.Add(ElementTarget.Array);
                else
                    targets.Add(ElementTarget.List);

                stripped = GraphValidation.EliminateNextWrapperFromCoreType(stripped, true, false, false);
            }

            var listMap = new ListMap()
            {
                CoreType = stripped,
                Elements = targets,
                ContainsArrayDeclarations = targets.Any(x => x == ElementTarget.Array),
            };

            _allMaps.TryAdd(targetType, listMap);
            return listMap;
        }

        private ListMap _targetMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListMangler" /> class.
        /// </summary>
        /// <param name="targetType">The concrete .NET type of the target list format.</param>
        public ListMangler(Type targetType)
        {
            this.TargetType = Validation.ThrowIfNullOrReturn(targetType, nameof(targetType));
            _targetMap = FindOrCreateMap(this.TargetType);
        }

        /// <summary>
        /// Converts the supplied list data structure into a compatible list
        /// structor for <see cref="TargetType"/>.
        /// </summary>
        /// <param name="listData">The list data. This list must be List{T} or
        /// successive chains of List{T}.</param>
        /// <returns>System.Object.</returns>
        public MangledResult Convert(object listData)
        {
            if (listData == null)
                return new MangledResult(null, false);

            // if there is a direct cast available (due to covariant relationships)
            // do it and skip all mangling
            var incomingType = listData.GetType();
            if (Validation.IsCastable(incomingType, this.TargetType))
                return new MangledResult(listData, false);

            // the incoming data must have the same level of "listing" as the target
            var incomingMap = FindOrCreateMap(incomingType);
            if (incomingMap.Elements.Count != _targetMap.Elements.Count)
            {
                throw new InvalidOperationException(
                    $"Cannot convert {incomingType.FriendlyName()} to {this.TargetType.FriendlyName()}. The types do not contain " +
                    "the same level of nested enumerables.");
            }

            // the type of data must be castable to the target type
            if (!Validation.IsCastable(incomingMap.CoreType, _targetMap.CoreType))
            {
                throw new InvalidOperationException(
                    $"Cannot convert a list of {incomingMap.CoreType.FriendlyName()} to a list of {_targetMap.CoreType.FriendlyName()}");
            }

            var dataOut = this.MangleData(listData, this.TargetType, 0);
            return new MangledResult(dataOut, true);
        }

        private object MangleData(object dataItem, Type target, int elementIndex)
        {
            if (!GraphValidation.IsValidListType(dataItem.GetType()))
            {
                // if the target index is not one beyond the end of the
                // the target map structure we have a problem.
                // This should be impossible
                // from the map checks...but just in case
                if (elementIndex != _targetMap.Elements.Count)
                    throw new GraphExecutionException("Invalid Object");

                return dataItem;
            }

            // listData should be a List<T>
            var enumerable = dataItem as IEnumerable;
            if (enumerable == null)
                throw new GraphExecutionException("Not IEnumerable");

            // for every item in this list
            // convert it into the type expected for the outbound set
            var typeT = target.GetEnumerableUnderlyingType();
            var mangledItems = new List<object>();
            foreach (var item in enumerable)
            {
                var mangledItem = this.MangleData(item, typeT, elementIndex + 1);
                mangledItems.Add(mangledItem);
            }

            // create the appropriate structure for the current level
            // of the listing structure
            if (_targetMap.Elements[elementIndex] == ElementTarget.Array)
                return this.CreateArray(typeT, mangledItems);
            else
                return this.CreateList(typeT, mangledItems);
        }

        private object CreateList(Type typeT, List<object> mangledItems)
        {
            var listType = typeof(List<>).MakeGenericType(typeT);
            var list = InstanceFactory.CreateInstance(listType) as IList;
            foreach (var item in mangledItems)
                list.Add(item);

            return list;
        }

        private object CreateArray(Type typeT, List<object> mangledItems)
        {
            var arr = Array.CreateInstance(typeT, mangledItems.Count);
            Array.Copy(mangledItems.ToArray(), arr, mangledItems.Count);

            return arr;
        }

        /// <summary>
        /// Gets the .NET type that list data will be converted to.
        /// </summary>
        /// <value>The type of the target.</value>
        public Type TargetType { get; }

        private enum ElementTarget
        {
            Array,
            List,
        }

        private class ListMap
        {
            public Type CoreType { get; set; }

            public List<ElementTarget> Elements { get; set; }

            public bool ContainsArrayDeclarations { get; set; }
        }

        /// <summary>
        /// The output of a <see cref="ListMangler"/> operation.
        /// </summary>
        public class MangledResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MangledResult"/> class.
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="wasMangled">if set to <c>true</c> [was mangled].</param>
            internal MangledResult(object data, bool wasMangled)
            {
                this.Data = data;
                this.IsChanged = wasMangled;
            }

            /// <summary>
            /// Gets the resultant data object that was created during mangling.
            /// </summary>
            /// <value>The data.</value>
            public object Data { get; }

            /// <summary>
            /// Gets a value indicating whether the output <see cref="Data"/> was
            /// altered or reconstructed from its constituent data.
            /// </summary>
            /// <value><c>true</c> if the data object was rebuilt; otherwise, <c>false</c>.</value>
            public bool IsChanged { get; }
        }
    }
}