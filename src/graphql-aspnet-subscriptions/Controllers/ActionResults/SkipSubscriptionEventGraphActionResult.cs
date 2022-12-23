// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Controllers.ActionResults
{
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Controllers;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// <para>
    /// An action result that, when returned from a subscription, will cause the
    /// event data to be dropped and no data will be sent to the connection client.
    /// </para>
    /// </summary>
    public class SkipSubscriptionEventGraphActionResult : IGraphActionResult
    {
        private readonly bool _shouldCompleteSubscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="SkipSubscriptionEventGraphActionResult"/> class.
        /// </summary>
        /// <param name="shouldCompleteSubscription">if this action result
        /// should also signal that the subscription be closed/completed when the event is ultimately
        /// skipped.</param>
        public SkipSubscriptionEventGraphActionResult(bool shouldCompleteSubscription = false)
        {
            _shouldCompleteSubscription = shouldCompleteSubscription;
        }

        /// <inheritdoc />
        public Task CompleteAsync(SchemaItemResolutionContext context)
        {
            if (context is FieldResolutionContext frc)
            {
                frc.Result = null;
                if (frc.Request.Field is ISubscriptionGraphField)
                {
                    frc.Session.Items.TryAdd(SubscriptionConstants.ContextDataKeys.SKIP_EVENT, true);
                    if (_shouldCompleteSubscription)
                        CompleteSubscriptionGraphActionResult.ConfigureForCompletedSubscription(frc);

                    return Task.CompletedTask;
                }
            }

            context.Cancel();
            context.Messages.Critical(
                $"Invalid Action Result. {nameof(SkipSubscriptionEventGraphActionResult)} can only " +
                "be used on subscription actions.",
                Constants.ErrorCodes.INVALID_ACTION_RESULT,
                context.Request.Origin);

            return Task.CompletedTask;
        }
    }
}