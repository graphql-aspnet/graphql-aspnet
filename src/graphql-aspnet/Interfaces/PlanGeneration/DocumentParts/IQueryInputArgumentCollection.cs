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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.PlanGeneration.Document.Parts;

    /// <summary>
    /// A collection of arguments parsed from a user's query document for a given field or directive.
    /// </summary>
    public interface IQueryInputArgumentCollection : IReadOnlyDictionary<string, QueryInputArgument>
    {
        /// <summary>
        /// Determines whether the specified input name exists on this collection.
        /// </summary>
        /// <param name="inputName">Name of the input argument.</param>
        /// <returns><c>true</c> if this instance contains the key; otherwise, <c>false</c>.</returns>
        bool ContainsKey(ReadOnlyMemory<char> inputName);

        /// <summary>
        /// Adds the input argument to the collection.
        /// </summary>
        /// <param name="argument">The argument.</param>
        void AddArgument(QueryInputArgument argument);

        /// <summary>
        /// Inspects the collection of arguments for an argument with the provided input name.
        /// Returns the argument if its found otherwise null.
        /// </summary>
        /// <param name="inputName">The name of the argument to look for.</param>
        /// <returns>The found argument or null.</returns>
        QueryInputArgument FindArgumentByName(ReadOnlyMemory<char> inputName);

        /// <summary>
        /// Inspects the collection of arguments for an argument with the provided input name.
        /// Returns the argument if its found otherwise null.
        /// </summary>
        /// <param name="inputName">The name of the argument to look for.</param>
        /// <returns>The found argument or null.</returns>
        QueryInputArgument FindArgumentByName(string inputName);
    }
}