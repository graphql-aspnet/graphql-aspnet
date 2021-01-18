// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.Subscriptions.Tests.Security.SecurityTestData
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Controllers;
    using GraphQL.AspNet.Interfaces.Controllers;
    using Microsoft.AspNetCore.Authorization;

    public class SecureSubscriptionWidgetController : GraphController
    {
        [Mutation("updateSecureWidget", typeof(SubscriptionSecureWidget))]
        [Description("Retrieves a single starship by its given id.")]
        public Task<IGraphActionResult> UpdateStarship(SubscriptionSecureWidget widget)
        {
            if (!ModelState.IsValid)
                return this.BadRequest(this.ModelState).AsCompletedTask();

            widget.Name += "_updated";

            // raise an event for any listening subscriptions
            this.PublishSubscriptionEvent("secureWidgetUpdated", widget);
            return this.Ok(widget).AsCompletedTask();
        }

        [Authorize("SecureWidgetPolicy")]
        [SubscriptionRoot("secureWidgetChanged", typeof(SubscriptionSecureWidget), EventName = "secureWidgetUpdated")]
        public Task<IGraphActionResult> SecuredStarshipUpdated(SubscriptionSecureWidget eventData, string nameLike = "*")
        {
            if (eventData != null && (nameLike == "*" || eventData.Name.Contains(nameLike)))
                return Task.FromResult(this.Ok(eventData));

            return this.Ok().AsCompletedTask();
        }

        [SubscriptionRoot("unsecureWidgetChanged", typeof(SubscriptionSecureWidget), EventName = "secureWidgetUpdated")]
        public Task<IGraphActionResult> UnSecureStarshipUpdated(SubscriptionSecureWidget eventData, string nameLike = "*")
        {
            if (eventData != null && (nameLike == "*" || eventData.Name.Contains(nameLike)))
                return Task.FromResult(this.Ok(eventData));

            return this.Ok().AsCompletedTask();
        }

        [Authorize("SecureWidgetPolicy")]
        [TypeExtension(typeof(SubscriptionSecureWidget), "secureDate", typeof(DateTime))]
        public IGraphActionResult SecureWidgetDate(SubscriptionSecureWidget widget)
        {
            return this.Ok(DateTime.UtcNow);
        }

        [TypeExtension(typeof(SubscriptionSecureWidget), "unsecureDate", typeof(DateTime))]
        public IGraphActionResult UnSecureWidgetDate(SubscriptionSecureWidget widget)
        {
            return this.Ok(DateTime.UtcNow);
        }
    }
}