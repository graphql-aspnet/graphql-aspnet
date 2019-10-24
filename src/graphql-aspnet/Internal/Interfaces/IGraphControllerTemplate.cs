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
    using GraphQL.AspNet.Internal.TypeTemplates;

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
        IEnumerable<IGraphTypeFieldTemplate> Actions { get; }

        /// <summary>
        /// Gets the extension methods that have been parsed and defined for the controller.
        /// </summary>
        /// <value>The fields.</value>
        IEnumerable<IGraphTypeFieldTemplate> Extensions { get; }
    }
}