// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Schemas.Generation.TypeTemplates
{
    using System;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Directives;
    using GraphQL.AspNet.Interfaces.Internal;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Schemas.TypeSystem.Scalars;

    /// <summary>
    /// A helper class for creating graph type templates from <see cref="Type"/> info classes.
    /// </summary>
    public static class GraphTypeTemplates
    {
        /// <summary>
        /// A default implementation that will create an appropriate <see cref="IGraphTypeTemplate" />
        /// for the given <paramref name="objectType" /> and <paramref name="kind" />.
        /// </summary>
        /// <remarks>
        /// WARNING: This method does not parse or validate the template, it just instantiates an appropriate template instance
        /// for the given <paramref name="objectType"/>.
        /// </remarks>
        /// <param name="objectType">The type info representing the object.</param>
        /// <param name="kind">The kind of template to make. Only used to differentiate INPUT_OBJECT from
        /// OBJECT. Ignored otherwise.</param>
        /// <returns>IGraphTypeTemplate.</returns>
        public static IGraphTypeTemplate CreateTemplate(Type objectType, TypeKind? kind = null)
        {
            if (objectType == null)
                return null;

            // attempt to turn "int" into "IntScalarType" when necessary
            objectType = GlobalTypes.FindBuiltInScalarType(objectType) ?? objectType;

            IGraphTypeTemplate template;
            if (Validation.IsCastable<IScalarGraphType>(objectType))
                template = new ScalarGraphTypeTemplate(objectType);
            else if (Validation.IsCastable<IGraphUnionProxy>(objectType))
                template = new UnionGraphTypeTemplate(objectType);
            else if (objectType.IsEnum)
                template = new EnumGraphTypeTemplate(objectType);
            else if (objectType.IsInterface)
                template = new InterfaceGraphTypeTemplate(objectType);
            else if (Validation.IsCastable<GraphDirective>(objectType))
                template = new GraphDirectiveTemplate(objectType);
            else if (Validation.IsCastable<GraphController>(objectType))
                template = new GraphControllerTemplate(objectType);
            else if (kind.HasValue && kind.Value == TypeKind.INPUT_OBJECT)
                template = new InputObjectGraphTypeTemplate(objectType);
            else
                template = new ObjectGraphTypeTemplate(objectType);

            return template;
        }
    }
}