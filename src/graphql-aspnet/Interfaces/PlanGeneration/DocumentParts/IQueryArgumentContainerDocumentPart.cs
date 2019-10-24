// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts
{
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// An interface denoting that this docuemnt part is a container for query arguments.
    /// </summary>
    public interface IQueryArgumentContainerDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Adds the argument to the collection of arguments on this instance.
        /// </summary>
        /// <param name="argument">The argument.</param>
        void AddArgument(QueryInputArgument argument);
    }
}