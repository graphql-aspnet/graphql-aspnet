// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

#pragma warning disable SA1600 // Elements should be documented
namespace GraphQL.AspNet.SubscriberLoadTest.Server
{
    /// <summary>
    /// A fake response payload to mimic the and structure
    /// of a graphql response via a rest call. This is to minimize payload serialization
    /// size and complexity as well as transmission across the wire as a
    /// factor influncing the test.
    /// </summary>
    public class RestResponsePayload
    {
        public DonutPayload Data { get; set; }
    }
}