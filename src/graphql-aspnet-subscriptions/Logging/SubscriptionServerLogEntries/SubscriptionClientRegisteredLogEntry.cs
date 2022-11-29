﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Logging.SubscriptionServerLogEntries
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Interfaces.Subscriptions;
    using GraphQL.AspNet.Logging.Common;

    /// <summary>
    /// Recorded when a new client is successfully created by a subscription server.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema for which the route was registered.</typeparam>
    public class SubscriptionClientRegisteredLogEntry<TSchema> : GraphLogEntry
        where TSchema : class, ISchema
    {
        private readonly string _clientTypeShortName;
        private readonly string _schemaTypeShortName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionClientRegisteredLogEntry{TSchema}" /> class.
        /// </summary>
        /// <param name="client">The client that was created.</param>
        public SubscriptionClientRegisteredLogEntry(
            ISubscriptionClientProxy client)
            : base(SubscriptionLogEventIds.SubscriptionClientRegistered)
        {
            _clientTypeShortName = client?.GetType().FriendlyName();
            _schemaTypeShortName = typeof(TSchema).FriendlyName();
            this.SchemaTypeName = typeof(TSchema).FriendlyName(true);
            this.ClientTypeName = client?.GetType().FriendlyName(true);
            this.ClientProtocol = client?.Protocol;
            this.ClientId = client?.Id.ToString();
        }

        /// <summary>
        /// Gets the <see cref="Type" /> name of the schema instance the pipeline was generated for.
        /// </summary>
        /// <value>The name of the schema type.</value>
        public string SchemaTypeName
        {
            get => this.GetProperty<string>(LogPropertyNames.SCHEMA_TYPE_NAME);
            private set => this.SetProperty(LogPropertyNames.SCHEMA_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> name of the subscription client that was registered.
        /// </summary>
        /// <value>The name of the client type.</value>
        public string ClientTypeName
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_TYPE_NAME);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_TYPE_NAME, value);
        }

        /// <summary>
        /// Gets the communications protocol that was negotiated for this client.
        /// </summary>
        /// <value>The client protocol.</value>
        public string ClientProtocol
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.CLIENT_PROTOCOL);
            private set => this.SetProperty(SubscriptionLogPropertyNames.CLIENT_PROTOCOL, value);
        }

        /// <summary>
        /// Gets the unique id of the client that was created.
        /// </summary>
        /// <value>The identifier.</value>
        public string ClientId
        {
            get => this.GetProperty<string>(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_ID);
            private set => this.SetProperty(SubscriptionLogPropertyNames.SUBSCRIPTION_CLIENT_ID, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var idTruncated = this.ClientId?.Length > 8 ? this.ClientId.Substring(0, 8) : this.ClientId;
            return $"New Client Registered | Schema: '{_schemaTypeShortName}', Type: '{_clientTypeShortName}' (Id: {idTruncated})";
        }
    }
}