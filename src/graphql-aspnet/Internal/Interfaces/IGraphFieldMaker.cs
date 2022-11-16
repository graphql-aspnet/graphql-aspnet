// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using GraphQL.AspNet.Engine.TypeMakers;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A "maker" that can generate fully qualified graph fields from a given template.
    /// </summary>
    public interface IGraphFieldMaker
    {
        /// <summary>
        /// Creates a single graph field from the provided template using hte rules of this maker and the contained schema.
        /// </summary>
        /// <param name="template">The template to generate a field from.</param>
        /// <returns>GraphFieldCreationResult.</returns>
        GraphFieldCreationResult<IGraphField> CreateField(IGraphFieldTemplate template);

        /// <summary>
        /// Creates a single graph field from the provided template using hte rules of this maker and the contained schema.
        /// </summary>
        /// <param name="template">The template to generate a field from.</param>
        /// <returns>GraphFieldCreationResult.</returns>
        GraphFieldCreationResult<IInputGraphField> CreateField(IInputGraphFieldTemplate template);
    }
}