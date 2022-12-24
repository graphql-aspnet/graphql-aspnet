﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Resolvers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Resolvables;
    using GraphQL.AspNet.Interfaces.Execution.Variables;

    /// <summary>
    /// A higher order resolver that coerces the provided source data into a list of
    /// items of the provided singular input value resolver.
    /// </summary>
    internal class ListInputValueResolver : IInputValueResolver
    {
        private readonly IInputValueResolver _itemResolver;
        private readonly Type _listItemType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListInputValueResolver" /> class.
        /// </summary>
        /// <param name="listItemType">The expected type of the item in the list.</param>
        /// <param name="itemResolver">The item resolver that can generate instances of <paramref name="listItemType"/>.</param>
        public ListInputValueResolver(Type listItemType, IInputValueResolver itemResolver)
        {
            _itemResolver = Validation.ThrowIfNullOrReturn(itemResolver, nameof(itemResolver));
            _listItemType = Validation.ThrowIfNullOrReturn(listItemType, nameof(listItemType));
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

            if (resolvableItem is IResolvableList resolvableList)
            {
                var listType = typeof(List<>).MakeGenericType(_listItemType);
                var listInstance = InstanceFactory.CreateInstance(listType) as IList;
                foreach (var item in resolvableList)
                {
                    var itemInstance = _itemResolver.Resolve(item, variableData);
                    listInstance.Add(itemInstance);
                }

                return listInstance;
            }

            return null;
        }
    }
}