// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Internal.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// A container of field templates.
    /// </summary>
    public interface IGraphTypeFieldTemplateContainer : IGraphTypeTemplate
    {
        /// <summary>
        /// Gets the explicitly and implicitly decalred fields found on this instance.
        /// </summary>
        /// <value>The methods.</value>
        IReadOnlyDictionary<string, IGraphTypeFieldTemplate> FieldTemplates { get; }
    }
}