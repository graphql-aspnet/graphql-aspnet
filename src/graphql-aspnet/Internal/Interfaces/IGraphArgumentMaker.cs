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
    /// A maker that can generate input arguments.
    /// </summary>
    public interface IGraphArgumentMaker
    {
        /// <summary>
        /// Creates a single graph field from the provided template using hte rules of this maker and the contained schema.
        /// </summary>
        /// <param name="owner">The schema item that owns, or is responsible for creating the new argument.</param>
        /// <param name="argumentTemplate">The template to generate a argument from.</param>
        /// <returns>IGraphField.</returns>
        GraphArgumentCreationResult CreateArgument(ISchemaItem owner, IGraphArgumentTemplate argumentTemplate);
    }
}