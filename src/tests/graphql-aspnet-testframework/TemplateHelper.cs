// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework
{
    using System;
    using System.Linq;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Internal.Interfaces;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Tests.Framework.PipelineContextBuilders;

    /// <summary>
    /// This class assists in making template instances not connected to the primary provider.
    /// </summary>
    public static class TemplateHelper
    {
        /// <summary>
        /// Generates a controller template.
        /// </summary>
        /// <typeparam name="TController">The type of the t controller.</typeparam>
        /// <returns>IGraphControllerTemplate.</returns>
        public static IGraphControllerTemplate CreateControllerTemplate<TController>()
             where TController : GraphController
        {
            GraphQLProviders.TemplateProvider.Clear();
            return GraphQLProviders.TemplateProvider.ParseType<TController>() as IGraphControllerTemplate;
        }

        /// <summary>
        /// Generates a a field template for a single controller action method. This method will not attempt to
        /// templatize the entire referenced controller, only the single method. Can be handy for testing error conditions
        /// without generating a lot controllers each with different errored methods. Mostly for internal library template testing use.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>IGraphControllerTemplate.</returns>
        public static IGraphTypeFieldTemplate CreateActionMethodTemplate<TController>(string methodName)
             where TController : GraphController
        {
            var template = new MockGraphControllerTemplate<TController>(methodName);
            template.Parse();
            template.ValidateOrThrow();
            return template.FieldTemplates.FirstOrDefault(x => x.Value.InternalName.Equals(methodName, StringComparison.OrdinalIgnoreCase)).Value;
        }

        /// <summary>
        /// Helper method to create a field template for a controller or object method/property. This method will search both the
        /// names of the fields as they would exist in an object graph as well as the declared names of methods/properties. THe first
        /// found match is returned.
        /// </summary>
        /// <typeparam name="TType">The graph type or controller where the field is defined.</typeparam>
        /// <param name="fieldOrMethodName">Name of the field as defined in the object graph or the name of the method/property.</param>
        /// <returns>IGraphTypeFieldTemplate.</returns>
        public static IGraphTypeFieldTemplate CreateFieldTemplate<TType>(string fieldOrMethodName)
        {
            var template = CreateGraphTypeTemplate<TType>() as IGraphTypeFieldTemplateContainer;

            // bit of a hack but it solves a lot of schema configuration differences that
            // can occur when setting up a test do to references occuring out of process
            foreach (var kvp in template.FieldTemplates)
            {
                if (string.Equals(kvp.Value.InternalName, fieldOrMethodName, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;

                if (string.Equals(kvp.Value.Route.Name, fieldOrMethodName, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }

            throw new ArgumentOutOfRangeException(nameof(fieldOrMethodName), $"Test Setup Error. No field,method or property named '{fieldOrMethodName}' was found on the template of type '{typeof(TType).FriendlyName()}'.");
        }

        /// <summary>
        /// Generates a template for a give type and kind combination.
        /// </summary>
        /// <typeparam name="TType">The graph type to template.</typeparam>
        /// <param name="kind">The kind.</param>
        /// <returns>IGraphItemTemplate.</returns>
        public static IGraphItemTemplate CreateGraphTypeTemplate<TType>(TypeKind? kind = null)
        {
            GraphQLProviders.TemplateProvider.CacheTemplates = false;
            return GraphQLProviders.TemplateProvider.ParseType<TType>(kind);
        }

        /// <summary>
        /// Creates a template of the given type in its "OBJECT" graph type representation.
        /// </summary>
        /// <typeparam name="TObject">The type to create a template of.</typeparam>
        /// <returns>IObjectGraphTypeTemplate.</returns>
        public static IObjectGraphTypeTemplate CreateObjectTemplate<TObject>()
           where TObject : class
        {
            return CreateGraphTypeTemplate<TObject>(TypeKind.OBJECT) as IObjectGraphTypeTemplate;
        }

        /// <summary>
        /// Creates a template of the given type in its "INPUT_OBJECT" graph type representation.
        /// </summary>
        /// <typeparam name="TObject">The type to create a template of.</typeparam>
        /// <returns>IObjectGraphTypeTemplate.</returns>
        public static IInputObjectGraphTypeTemplate CreateInputObjectTemplate<TObject>()
           where TObject : class
        {
            return CreateGraphTypeTemplate<TObject>(TypeKind.INPUT_OBJECT) as IInputObjectGraphTypeTemplate;
        }

        /// <summary>
        /// Creates a template of the given enum value.
        /// </summary>
        /// <typeparam name="TEnum">The enum to template.</typeparam>
        /// <returns>IObjectGraphTypeTemplate.</returns>
        public static IEnumGraphTypeTemplate CreateEnumTemplate<TEnum>()
            where TEnum : Enum
        {
            return CreateGraphTypeTemplate<TEnum>(TypeKind.ENUM) as IEnumGraphTypeTemplate;
        }

        /// <summary>
        /// Creates a template of the given interface.
        /// </summary>
        /// <typeparam name="TInterface">The interface to template.</typeparam>
        /// <returns>IObjectGraphTypeTemplate.</returns>
        public static IInterfaceGraphTypeTemplate CreateInterfaceTemplate<TInterface>()
        {
            return CreateGraphTypeTemplate<TInterface>(TypeKind.INTERFACE) as IInterfaceGraphTypeTemplate;
        }
    }
}