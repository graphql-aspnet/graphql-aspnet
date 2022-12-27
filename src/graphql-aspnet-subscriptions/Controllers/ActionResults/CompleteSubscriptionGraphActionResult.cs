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
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Schema.TypeSystem;

    /// <summary>
    /// A result indicating an positive return status and an object (or null) to be resolved
    /// for the field. Additionally, will indicate to the subscription server
    /// that once this instance completes the subscription should be closed and no
    /// additional events will be raised.
    /// </summary>
    [DebuggerDisplay("Has Object: {_result?.GetType().FriendlyName()}")]
    public class CompleteSubscriptionGraphActionResult : ObjectReturnedGraphActionResult
    {
        /// <summary>
        /// Applies the appropriate session information to the field context to instruct
        /// the subscription server to complete/close the subscription.
        /// </summary>
        /// <param name="fieldContext">The field context.</param>
        internal static void ConfigureForCompletedSubscription(FieldResolutionContext fieldContext)
        {
            Validation.ThrowIfNull(fieldContext, nameof(fieldContext));
            fieldContext.Session.Items.TryAdd(SubscriptionConstants.ContextDataKeys.COMPLETE_SUBSCRIPTION, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteSubscriptionGraphActionResult"/> class.
        /// </summary>
        /// <param name="objectToReturn">The object to return.</param>
        public CompleteSubscriptionGraphActionResult(object objectToReturn)
            : base(objectToReturn)
        {
        }

        /// <inheritdoc />
        public override Task CompleteAsync(SchemaItemResolutionContext context)
        {
            if (context is FieldResolutionContext frc)
            {
                if (frc.Request.Field is ISubscriptionGraphField)
                {
                    ConfigureForCompletedSubscription(frc);
                    return base.CompleteAsync(context);
                }

                frc.Result = null;
            }

            context.Cancel();
            context.Messages.Critical(
                $"Invalid Action Result. {nameof(CompleteSubscriptionGraphActionResult)} can only " +
                "be used on subscription actions.",
                Constants.ErrorCodes.INVALID_ACTION_RESULT,
                context.Request.Origin);

            return Task.CompletedTask;
        }
    }
}