// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.TypeSystem
{
    using System.Collections.Generic;

    /// <summary>
    /// A collection of indexed <see cref="IEnumValue"/> objects owned by a single <see cref="IEnumGraphType"/>.
    /// </summary>
    public interface IEnumValueCollection : IReadOnlyDictionary<string, IEnumValue>
    {
        /// <summary>
        /// Attempts to find an enum value option that matches the supplied
        /// .NET enum definition. (e.g. MyEnum.Value1). If the supplied value is of the
        /// wrong enum type or the value is not defined on the enum list null is returned.
        /// </summary>
        /// <param name="enumValue">The enum value to search for.</param>
        /// <returns>IEnumValue.</returns>
        IEnumValue FindByEnumValue(object enumValue);
    }
}