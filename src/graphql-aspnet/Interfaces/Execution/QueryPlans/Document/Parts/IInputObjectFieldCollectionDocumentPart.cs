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
    public interface IInputObjectFieldCollectionDocumentPart : IReadOnlyDictionary<string, IInputObjectFieldDocumentPart>
    {
        /// <summary>
        /// Determines whether the specified field name exists on this collection.
        /// </summary>
        /// <param name="inputName">Name of the input field.</param>
        /// <returns><c>true</c> if this instance contains the key; otherwise, <c>false</c>.</returns>
        bool ContainsKey(ReadOnlyMemory<char> inputName);

        /// <summary>
        /// Searches for a field with the provided name.
        /// Returns the field if its found, otherwise null.
        /// </summary>
        /// <param name="name">The name of the field to look for.</param>
        /// <returns>The found argument or null.</returns>
        IInputObjectFieldDocumentPart FindFieldByName(ReadOnlyMemory<char> name);

        /// <summary>
        /// Searches for an field with the provided name.
        /// Returns the field if its found, otherwise null.
        /// </summary>
        /// <param name="name">The name of the field to look for.</param>
        /// <returns>The found field or null.</returns>
        IInputObjectFieldDocumentPart FindFieldByName(string name);

        /// <summary>
        /// Determines whether the specified field name is considered unique within the given set.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns><c>true</c> if the specified name is unique; otherwise, <c>false</c>.</returns>
        bool IsUnique(ReadOnlySpan<char> name);

        /// <summary>
        /// Determines whether the specified field name is considered unique within the given set.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns><c>true</c> if the specified name is unique; otherwise, <c>false</c>.</returns>
        bool IsUnique(string name);
    }
}