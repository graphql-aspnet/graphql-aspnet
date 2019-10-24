// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Interfaces.Variables
{
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;

    /// <summary>
    /// A user supplied variable value,in its most raw format devoid of any contextual information
    /// such as graph type. Essentially just a key/value pair.
    /// </summary>
    public interface IInputVariable : IResolvableItem
    {
        /// <summary>
        /// Gets the name of the variable as it was declared in the user's supplied data.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
    }
}