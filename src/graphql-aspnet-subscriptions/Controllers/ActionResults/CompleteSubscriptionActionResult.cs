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
    using System.Diagnostics;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;

    /// <summary>
    /// A result indicating an positive return status and an object (or null) to be resolved
    /// for the field. Additionally, will indicate to the subscription server
    /// that once this instance completes the subscription should be closed and no
    /// additional events will be raised.
    /// </summary>
    [DebuggerDisplay("Has Object: {_result?.GetType().FriendlyName()}")]
    public class CompleteSubscriptionActionResult : ObjectReturnedGraphActionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteSubscriptionActionResult"/> class.
        /// </summary>
        /// <param name="objectToReturn">The object to return.</param>
        public CompleteSubscriptionActionResult(object objectToReturn)
            : base(objectToReturn)
        {
        }

        /// <inheritdoc />
        public override Task Complete(BaseResolutionContext context)
        {
            if (context is FieldResolutionContext frc && frc.Request.Field is ISubscriptionGraphField)
            {
                frc.Request.Items.TryAdd(SubscriptionConstants.Execution.COMPLETED_SUBSCRIPTION_KEY, true);
            }
            else
            {
                context.Messages.Critical(
                    $"Invalid Action Result. {nameof(CompleteSubscriptionActionResult)} can only " +
                    "be used on subscription action methods.",
                    Constants.ErrorCodes.INVALID_ACTION_RESULT,
                    context.Request.Origin);
            }

            return base.Complete(context);
        }
    }
}