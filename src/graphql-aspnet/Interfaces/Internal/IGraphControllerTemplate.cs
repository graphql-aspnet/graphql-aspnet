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
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;

    /// <summary>
    /// A special marker interface that identifies a valid <see cref="IObjectGraphTypeTemplate"/>
    /// as a <see cref="GraphControllerTemplate"/>.
    /// </summary>
    public interface IGraphControllerTemplate : IObjectGraphTypeTemplate
    {
        /// <summary>
        /// Gets the actions that have been parsed and defined for the controller.
        /// </summary>
        /// <value>The fields.</value>
        IEnumerable<IGraphFieldTemplate> Actions { get; }

        /// <summary>
        /// Gets the extension methods that have been parsed and defined for the controller.
        /// </summary>
        /// <value>The fields.</value>
        IEnumerable<IGraphFieldTemplate> Extensions { get; }
    }
}