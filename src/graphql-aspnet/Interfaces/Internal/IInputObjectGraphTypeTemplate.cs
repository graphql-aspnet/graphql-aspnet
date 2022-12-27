// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Internal
{
    using System.Collections.Generic;

    /// <summary>
    /// An interface describing qualified Graph object.
    /// </summary>
    public interface IInputObjectGraphTypeTemplate : IGraphTypeTemplate
    {
        /// <summary>
        /// Gets the explicitly and implicitly decalred fields found on this instance.
        /// </summary>
        /// <value>The fields declared on this graph type template.</value>
        IReadOnlyDictionary<string, IInputGraphFieldTemplate> FieldTemplates { get; }
    }
}