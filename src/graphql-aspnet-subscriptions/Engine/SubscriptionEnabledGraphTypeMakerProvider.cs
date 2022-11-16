// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Defaults
{
    using GraphQL.AspNet.Defaults.TypeMakers;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.Internal.Interfaces;

    /// <summary>
    /// An upgraded "type maker" factory that adds low level subscription field support
    /// to the type system.
    /// </summary>
    public class SubscriptionEnabledGraphTypeMakerProvider : DefaultGraphTypeMakerProvider
    {
        /// <inheritdoc />
        public override IGraphFieldMaker CreateFieldMaker(ISchema schema)
        {
            return new SubscriptionEnabledGraphFieldMaker(schema);
        }
    }
}