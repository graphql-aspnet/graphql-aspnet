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
        internal void AddArgument(IQueryArgumentDocumentPart argument);

        /// <summary>
        /// Gets a collection of input arguments arguments that have been declared in the query document that should be
        /// applied to this field.
        /// </summary>
        /// <value>The arguments.</value>
        IQueryInputArgumentCollectionDocumentPart Arguments { get; }
    }
}