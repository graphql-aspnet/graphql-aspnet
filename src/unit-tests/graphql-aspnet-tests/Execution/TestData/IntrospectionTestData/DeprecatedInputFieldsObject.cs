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

    /// <summary>
    /// A test input object with deprecated fields for testing introspection.
    /// </summary>
    [GraphType(InputName = "DeprecatedFieldsInput")]
    public class DeprecatedInputFieldsObject
    {
        /// <summary>
        /// Gets or sets an active field that is not deprecated.
        /// </summary>
        [GraphField]
        public string ActiveField { get; set; }

        /// <summary>
        /// Gets or sets a legacy field that is deprecated.
        /// </summary>
        [GraphField]
        [Deprecated("Use activeField instead")]
        public string LegacyField { get; set; }

        /// <summary>
        /// Gets or sets another active field.
        /// </summary>
        [GraphField]
        public int Count { get; set; }
    }
}
