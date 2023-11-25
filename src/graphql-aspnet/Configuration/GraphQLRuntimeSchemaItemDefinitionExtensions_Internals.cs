// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Configuration
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution;
    using GraphQL.AspNet.Interfaces.Schema.RuntimeDefinitions;
    using GraphQL.AspNet.Schemas.Generation.RuntimeSchemaItemDefinitions;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Extension methods for configuring minimal API methods as fields on the graph.
    /// </summary>
    public static partial class GraphQLRuntimeSchemaItemDefinitionExtensions
    {
        private static TItemType RequireAuthorizationInternal<TItemType>(
            this TItemType schemaItem,
            string policyName = null,
            string roles = null)
            where TItemType : IGraphQLRuntimeSchemaItemDefinition
        {
            Validation.ThrowIfNull(schemaItem, nameof(schemaItem));

            var attrib = new AuthorizeAttribute();
            attrib.Policy = policyName?.Trim();
            attrib.Roles = roles?.Trim();
            schemaItem.AddAttribute(attrib);

            // remove any allow anon attriubtes
            var allowAnonAttribs = schemaItem.Attributes.OfType<AllowAnonymousAttribute>().ToList();
            foreach (var anonAttrib in allowAnonAttribs)
                schemaItem.RemoveAttribute(anonAttrib);

            return schemaItem;
        }

        private static TItemType AllowAnonymousInternal<TItemType>(TItemType schemaItem)
            where TItemType : IGraphQLRuntimeSchemaItemDefinition
        {
            Validation.ThrowIfNull(schemaItem, nameof(schemaItem));
            if (schemaItem.Attributes.Count(x => x is AllowAnonymousAttribute) == 0)
            {
                schemaItem.AddAttribute(new AllowAnonymousAttribute());
            }

            // remove any authorize attributes
            var authAttribs = schemaItem.Attributes.OfType<AuthorizeAttribute>().ToList();
            foreach (var attib in authAttribs)
                schemaItem.RemoveAttribute(attib);

            return schemaItem;
        }

        private static TItemType ClearPossibleTypesInternal<TItemType>(TItemType fieldBuilder)
            where TItemType : IGraphQLRuntimeSchemaItemDefinition
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));
            var attributes = fieldBuilder.Attributes.OfType<PossibleTypesAttribute>().ToList();
            foreach (var att in attributes)
                fieldBuilder.RemoveAttribute(att);

            return fieldBuilder;
        }

        private static TItemType AddPossibleTypesInternal<TItemType>(TItemType fieldBuilder, Type firstPossibleType, params Type[] additionalPossibleTypes)
            where TItemType : IGraphQLRuntimeSchemaItemDefinition
        {
            Validation.ThrowIfNull(fieldBuilder, nameof(fieldBuilder));
            var possibleTypes = new PossibleTypesAttribute(firstPossibleType, additionalPossibleTypes);
            fieldBuilder.AddAttribute(possibleTypes);
            return fieldBuilder;
        }

        private static IGraphQLRuntimeFieldGroupDefinition MapGraphQLFieldInternal(
            SchemaOptions schemaOptions,
            GraphOperationType operationType,
            string pathTemplate)
        {
            schemaOptions = Validation.ThrowIfNullOrReturn(schemaOptions, nameof(schemaOptions));
            pathTemplate = Validation.ThrowIfNullWhiteSpaceOrReturn(pathTemplate, nameof(pathTemplate));

            var fieldTemplate = new RuntimeFieldGroupTemplate(
                schemaOptions,
                (ItemPathRoots)operationType,
                pathTemplate);

            return fieldTemplate;
        }

        private static TItemType AddResolverInternal<TItemType>(this TItemType fieldBuilder, Type expectedReturnType, string unionName, Delegate resolverMethod)
            where TItemType : IGraphQLResolvableSchemaItemDefinition
        {
            fieldBuilder.Resolver = resolverMethod;
            fieldBuilder.ReturnType = expectedReturnType;

            // since the resolver was declared as non-union, remove any potential union setup that might have
            // existed via a previous call. if TReturnType is a union proxy it will be
            // picked up automatically during templating
            var unionAttrib = fieldBuilder.Attributes.OfType<UnionAttribute>().SingleOrDefault();
            if (string.IsNullOrEmpty(unionName))
            {
                if (unionAttrib != null)
                    fieldBuilder.RemoveAttribute(unionAttrib);
            }
            else if (unionAttrib != null)
            {
                unionAttrib.UnionName = unionName?.Trim();
                unionAttrib.UnionMemberTypes.Clear();
            }
            else
            {
                fieldBuilder.AddAttribute(new UnionAttribute(unionName.Trim()));
            }

            return fieldBuilder;
        }
    }
}