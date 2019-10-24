// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a resolvable item that is a collection of key/value pairs. This item type is
    /// typically resolved into an object with its KVPs representing properties of the object to be created.
    /// </summary>
    public interface IResolvableFieldSet : IResolvableItem
    {
        /// <summary>
        /// Gets the collection of fields defined on this instance.
        /// </summary>
        /// <value>The fields.</value>
        IEnumerable<KeyValuePair<string, IResolvableItem>> Fields { get; }

        /// <summary>
        /// Attempts to retrieve a field by its name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="field">The field that was found, if any.</param>
        /// <returns><c>true</c> if the field was found and successfully returned, <c>false</c> otherwise.</returns>
        bool TryGetField(string fieldName, out IResolvableItem field);
    }
}