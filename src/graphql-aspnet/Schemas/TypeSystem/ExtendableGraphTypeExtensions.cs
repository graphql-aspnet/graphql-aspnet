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
    using System.Threading.Tasks;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Execution.Resolvers;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.Structural;

    /// <summary>
    /// Extension methods for quickly adding new fields to extendable graph types.
    /// </summary>
    public static class ExtendableGraphTypeExtensions
    {
        /// <summary>
        /// Creates and adds a new <see cref="IGraphField" /> to the growing collection.
        /// </summary>
        /// <typeparam name="TReturn">The expected type of data to be returned from this field.</typeparam>
        /// <param name="graphType">The graph type being extended.</param>
        /// <param name="fieldName">The formatted name of the field as it will appear in the object graph.</param>
        /// <param name="typeExpression">The item representing how this field returns a graph type.</param>
        /// <param name="resolver">The resolver used to fulfil requests to this field.</param>
        /// <param name="description">The description to assign to the field.</param>
        /// <returns>IGraphTypeField.</returns>
        public static IGraphField Extend<TReturn>(
            this IExtendableGraphType graphType,
            string fieldName,
            GraphTypeExpression typeExpression,
            Func<object, Task<TReturn>> resolver,
            string description = null)
        {
            return graphType.Extend<object, TReturn>(
                fieldName,
                typeExpression,
                resolver,
                description);
        }

        /// <summary>
        /// Creates and adds a new <see cref="IGraphField" /> to the growing collection.
        /// </summary>
        /// <typeparam name="TSource">The expected type of the source data supplied to the resolver.</typeparam>
        /// <typeparam name="TReturn">The expected type of data to be returned from the field.</typeparam>
        /// <param name="graphType">The graph type being extended.</param>
        /// <param name="fieldName">The formatted name of the field as it will appear in the object graph.</param>
        /// <param name="typeExpression">The item representing how this field returns a graph type.</param>
        /// <param name="resolver">The resolver used to fulfil requests to this field.</param>
        /// <param name="description">The description to assign to the field.</param>
        /// <returns>IGraphTypeField.</returns>
        public static IGraphField Extend<TSource, TReturn>(
            this IExtendableGraphType graphType,
            string fieldName,
            GraphTypeExpression typeExpression,
            Func<TSource, Task<TReturn>> resolver,
            string description = null)
            where TSource : class
        {
            Validation.ThrowIfNullOrReturn(graphType, nameof(graphType));
            fieldName = Validation.ThrowIfNullWhiteSpaceOrReturn(fieldName, nameof(fieldName));

            var fieldRoute = graphType.Route.CreateChild(fieldName);

            var field = new MethodGraphField(
                fieldName,
                $"GraphQLExtendedField",
                typeExpression,
                fieldRoute,
                typeof(TReturn),
                GraphValidation.EliminateNextWrapperFromCoreType(typeof(TReturn)),
                FieldResolutionMode.PerSourceItem,
                new FunctionGraphFieldResolver<TSource, TReturn>(resolver));
            field.Description = description;

            return graphType.Extend(field);
        }
    }
}