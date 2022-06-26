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

    /// <summary>
    /// A collection of arguments parsed from a user's query document for a given field or directive.
    /// </summary>
    public interface IInputArgumentCollectionDocumentPart : IReadOnlyDictionary<string, IInputArgumentDocumentPart>
    {
        /// <summary>
        /// Determines whether the specified input name exists on this collection.
        /// </summary>
        /// <param name="inputName">Name of the input argument.</param>
        /// <returns><c>true</c> if this instance contains the key; otherwise, <c>false</c>.</returns>
        bool ContainsKey(ReadOnlyMemory<char> inputName);

        /// <summary>
        /// Searches for an argument with the provided name.
        /// Returns the argument if its found otherwise null.
        /// </summary>
        /// <param name="name">The name of the argument to look for.</param>
        /// <returns>The found argument or null.</returns>
        IInputArgumentDocumentPart FindArgumentByName(ReadOnlyMemory<char> name);

        /// <summary>
        /// Searches for an argument with the provided name.
        /// Returns the argument if its found otherwise null.
        /// </summary>
        /// <param name="name">The name of the argument to look for.</param>
        /// <returns>The found argument or null.</returns>
        IInputArgumentDocumentPart FindArgumentByName(string name);
    }
}