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
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;

    /// <summary>
    /// <para>
    /// An action result that, when returned from a subscription, will cause the
    /// event data to be dropped and no data will be sent to the connection client.
    /// </para>
    /// </summary>
    public class SkipSubscriptionEventActionResult : IGraphActionResult
    {
        /// <inheritdoc />
        public Task Complete(BaseResolutionContext context)
        {
            if (context is FieldResolutionContext frc && frc.Request.Field is ISubscriptionGraphField)
            {
                frc.Request.Items.TryAdd(SubscriptionConstants.Execution.SKIPPED_EVENT_KEY, true);
                frc.Result = null;
                return Task.CompletedTask;
            }

            context.Messages.Critical(
                $"Invalid Action Result. {nameof(SkipSubscriptionEventActionResult)} can only " +
                "be used on subscription actions.",
                Constants.ErrorCodes.INVALID_ACTION_RESULT,
                context.Request.Origin);

            return Task.CompletedTask;
        }
    }
}