// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Execution.QueryPlans.DocumentParts
{
    using GraphQL.AspNet.Schemas;

    /// <summary>
    /// A meta document part representing an input with a potential supplied value.
    /// </summary>
    public interface IInputValueDocumentPart : IDocumentPart
    {
        /// <summary>
        /// Gets the name of this input document part in the query document.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Gets the supplied value to this input object field.
        /// </summary>
        /// <value>The value.</value>
        ISuppliedValueDocumentPart Value { get; }

        /// <summary>
        /// Gets the type expression assigned to this input value. Usually the type expression
        /// of the input object field or the argument on a field or directive.
        /// </summary>
        /// <value>The type expression.</value>
        GraphTypeExpression TypeExpression { get; }
    }
}