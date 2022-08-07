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
    public interface IResolvableFieldSet : IResolvableValueItem
    {
        /// <summary>
        /// Attempts to retrieve a resolvable field with the given name from this set of
        /// items.
        /// </summary>
        /// <param name="fieldName">Name of the field to find.</param>
        /// <param name="foundField">When found, the field is assigned to this parameter.</param>
        /// <returns><c>true</c> if the field was found, <c>false</c> otherwise.</returns>
        bool TryGetField(string fieldName, out IResolvableValueItem foundField);

        /// <summary>
        /// Gets the collection of fields defined on this instance.
        /// </summary>
        /// <value>The fields.</value>
        IEnumerable<KeyValuePair<string, IResolvableValueItem>> ResolvableFields { get; }
    }
}