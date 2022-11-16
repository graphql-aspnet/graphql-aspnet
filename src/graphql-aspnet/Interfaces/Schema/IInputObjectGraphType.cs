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
    /// <summary>
    /// A representation of a complex object type known to a schema, used in an "input" scenario.
    /// </summary>
    public interface IInputObjectGraphType : IGraphType, ITypedSchemaItem
    {
        /// <summary>
        /// Gets a collection of fields made available by this interface.
        /// </summary>
        /// <value>The fields.</value>
        IReadOnlyInputGraphFieldCollection Fields { get; }

        /// <summary>
        /// Gets the <see cref="IInputGraphField"/> with the specified field name.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns>IGraphTypeField.</returns>
        IInputGraphField this[string fieldName] { get; }
    }
}