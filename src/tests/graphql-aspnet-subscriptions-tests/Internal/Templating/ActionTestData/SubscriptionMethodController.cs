// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Internal.Templating.ActionTestData
{
    using System.ComponentModel;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Tests.Framework.CommonHelpers;

    public class SubscriptionMethodController : GraphController
    {
        [Subscription("path1", EventName = "WatchForThings")]
        [Description("SubDescription")]
        public TwoPropertyObject SingleMethod(TwoPropertyObject data)
        {
            return data;
        }

        [Subscription("path2", EventName = "Invalid$Event Name")]
        [Description("SubDescription")]
        public TwoPropertyObject InvalidEventNameMethod(TwoPropertyObject data)
        {
            return data;
        }

        // this method has no input of the output type
        // and no param marked as an explicit source
        [Subscription("path3")]
        public TwoPropertyObject MissingInputReferenceObject(TwoPropertyObjectV2 data)
        {
            return null;
        }

        [Subscription("path4")]
        public TwoPropertyObject ExplicitSourceReference([SubscriptionSource] TwoPropertyObjectV2 data)
        {
            return null;
        }

        [Subscription("path5")]
        public TwoPropertyObject DeclaredSourceFirst([SubscriptionSource] TwoPropertyObjectV2 data, TwoPropertyObject data2)
        {
            return null;
        }

        [Subscription("path5")]
        public TwoPropertyObject DeclaredSourceSecond(TwoPropertyObject data2, [SubscriptionSource] TwoPropertyObjectV2 data)
        {
            return null;
        }

        [Subscription("path6")]
        public TwoPropertyObject MultipleDeclaredSourceParams([SubscriptionSource] TwoPropertyObject data2, [SubscriptionSource] TwoPropertyObjectV2 data)
        {
            return null;
        }
    }
}