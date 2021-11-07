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
    using System;
    using System.Collections.Generic;
    using GraphQL.AspNet.Interfaces.Execution;

    /// <summary>
    /// A representation of an enumeration, a fixed set of possible values.
    /// </summary>
    /// <seealso cref="IGraphType" />
    public interface IEnumGraphType : IGraphType, ITypedSchemaItem
    {
        /// <summary>
        /// Adds a new value options to this ENUM graph type. If an enum with the given name
        /// already exists an exception is thrown.
        /// </summary>
        /// <param name="option">The option.</param>
        void AddOption(IEnumOption option);

        /// <summary>
        /// Removes an option from the valid list of enum options.
        /// </summary>
        /// <param name="name">The name of hte enum option (case insensitive).</param>
        /// <returns>The enum option that was found and removed from the graph type. Returns null,
        /// if no option matching the supplied <paramref name="name"/> was found.</returns>
        IEnumOption RemoveOption(string name);

        /// <summary>
        /// Gets the values that can be supplied to this enum.
        /// </summary>
        /// <value>The values.</value>
        IReadOnlyDictionary<string, IEnumOption> Values { get; }

        /// <summary>
        /// Gets or sets an object that will perform a conversion of raw data into a valid
        /// <see cref="Enum"/> defined by this graph type.
        /// </summary>
        /// <value>The resolver assigned to this instance.</value>
        ILeafValueResolver SourceResolver { get; set; }
    }
}