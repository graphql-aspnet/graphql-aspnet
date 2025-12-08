// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Execution.TestData.IntrospectionTestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;

    /// <summary>
    /// A test controller that uses the DeprecatedInputFieldsObject as an input type.
    /// </summary>
    public class DeprecatedInputFieldsController : GraphController
    {
        /// <summary>
        /// A mutation that accepts the input object with deprecated fields.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <returns>The active field value.</returns>
        [MutationRoot]
        public string ProcessInput(DeprecatedInputFieldsObject input)
        {
            return input?.ActiveField;
        }
    }
}
