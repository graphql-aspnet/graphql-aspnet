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
    using System.Collections;
    using System.Collections.Generic;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Generics;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.Variables;

    /// <summary>
    /// A higher order resolver that coerces the provided source data into a list of items of the provided singular value resolver.
    /// </summary>
    public class ListValueResolver : BaseValueResolver, IInputValueResolver
    {
        private readonly IInputValueResolver _itemResolver;
        private readonly Type _listItemType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListValueResolver" /> class.
        /// </summary>
        /// <param name="listItemType">The expected type of the item in the list.</param>
        /// <param name="itemResolver">The item resolver that can generate instances of <paramref name="listItemType"/>.</param>
        public ListValueResolver(Type listItemType, IInputValueResolver itemResolver)
        {
            _itemResolver = Validation.ThrowIfNullOrReturn(itemResolver, nameof(itemResolver));
            _listItemType = Validation.ThrowIfNullOrReturn(listItemType, nameof(listItemType));
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
            var resolver = new ListValueResolver(_listItemType, _itemResolver);
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
            if (resolvableItem is IResolvableList resolvableList)
            {
                var listType = typeof(List<>).MakeGenericType(_listItemType);
                var listInstance = InstanceFactory.CreateInstance(listType) as IList;
                foreach (var item in resolvableList.ListItems)
                {
                    listInstance.Add(_itemResolver.WithVariables(this.VariableCollection).Resolve(item));
                }

                return listInstance;
            }

            return null;
        }
    }
}