// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples
{
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples.Common;
    using GraphQL.AspNet.SubscriberLoadTest.TestConsole.Clients.GraphQLSamples.Responses;
    using GQLRequest = GraphQL.GraphQLRequest;

    /// <summary>
    /// A script that listens for data on a subscription connection.
    /// </summary>
    public class OnDonutUpdatedSubscription : GraphQLSubscriptionBase<OnDonutUpdatedResponse>
    {
        private const string SUBSCRIPTION_TEXT_WITH_ID =
            "subscription {{ " +
            "    onDonutUpdated(id: \"{0}\") {{ " +
            "       id " +
            "       name " +
            "       flavor " +
            "   }}" +
            "}}";

        private const string SUBSCRIPTION_TEXT =
           "subscription { " +
           "    onDonutUpdated(id: \"1\") { " +
           "       id " +
           "       name " +
           "       flavor " +
           "   }" +
           "}";

        /// <summary>
        /// Initializes a new instance of the <see cref="OnDonutUpdatedSubscription" /> class.
        /// </summary>
        /// <param name="url">The url to send graphql requests to.</param>
        /// <param name="scriptNumber">The individual script number of this instance.</param>
        /// <param name="categorySuffix">The execution category suffix
        /// to apply to this individual script instance.</param>
        public OnDonutUpdatedSubscription(
            string url,
            int scriptNumber,
            string categorySuffix = "")
            : base(url, scriptNumber, "OnDonutUpdated", categorySuffix)
        {
        }

        /// <inheritdoc />
        protected override GQLRequest CreateRequest()
        {
            var query = string.Format(SUBSCRIPTION_TEXT_WITH_ID, this.ClientNumber);

            // var query = SUBSCRIPTION_TEXT;
            return new GQLRequest(query);
        }

        /// <inheritdoc />
        protected override bool ValidateResult(OnDonutUpdatedResponse response)
        {
            return response != null
                && response.OnDonutUpdated != null
                && !string.IsNullOrWhiteSpace(response.OnDonutUpdated.Id)
                && !string.IsNullOrWhiteSpace(response.OnDonutUpdated.Name)
                && !string.IsNullOrWhiteSpace(response.OnDonutUpdated.Flavor);
        }
    }
}