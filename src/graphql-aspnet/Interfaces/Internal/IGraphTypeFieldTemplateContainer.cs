﻿// *************************************************************
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
    /// A container of field templates.
    /// </summary>
    public interface IGraphTypeFieldTemplateContainer : IGraphTypeTemplate
    {
        /// <summary>
        /// Gets the explicitly and implicitly decalred fields found on this instance.
        /// </summary>
        /// <value>The fields declared on this template.</value>
        IReadOnlyList<IGraphFieldTemplate> FieldTemplates { get; }
    }
}