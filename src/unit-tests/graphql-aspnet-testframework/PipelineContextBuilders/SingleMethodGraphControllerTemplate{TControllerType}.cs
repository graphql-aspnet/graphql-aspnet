// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Framework.PipelineContextBuilders
{
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Schemas.Generation.TypeTemplates;

    /// <summary>
    /// A mocked controller template that will selectively parse actions instead of the whole template.
    /// </summary>
    /// <typeparam name="TControllerType">The type of the controller to templatize.</typeparam>
    public class SingleMethodGraphControllerTemplate<TControllerType> : SingleMethodGraphControllerTemplate
        where TControllerType : GraphController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleMethodGraphControllerTemplate{TControllerType}"/> class.
        /// </summary>
        /// <param name="methodName">Name of the single action method to parse. When not
        /// provided (e.g. <c>null</c>) this template will function the same as <see cref="GraphControllerTemplate"/>
        /// and all methods will be parsed.</param>
        public SingleMethodGraphControllerTemplate(string methodName = null)
             : base(typeof(TControllerType), methodName)
        {
        }
    }
}