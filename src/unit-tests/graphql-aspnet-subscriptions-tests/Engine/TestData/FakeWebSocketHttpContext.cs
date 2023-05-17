// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Engine.TestData
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentConstruction.Steps;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;

    public class FakeWebSocketHttpContext : HttpContext
    {
        private DefaultHttpContext _subcontext;

        public FakeWebSocketHttpContext()
        {
            _subcontext = new DefaultHttpContext();
        }

        public override WebSocketManager WebSockets => new FakeWebSocketManager();

        public override IFeatureCollection Features => _subcontext.Features;

        public override HttpRequest Request => _subcontext.Request;

        public override HttpResponse Response => _subcontext.Response;

        public override ConnectionInfo Connection => _subcontext.Connection;

        public override ClaimsPrincipal User
        {
            get => _subcontext.User;
            set => _subcontext.User = value;
        }

        public override IDictionary<object, object> Items
        {
            get => _subcontext.Items;
            set => _subcontext.Items = value;
        }

        public override IServiceProvider RequestServices
        {
            get => _subcontext.RequestServices;
            set => _subcontext.RequestServices = value;
        }

        public override CancellationToken RequestAborted
        {
            get => _subcontext.RequestAborted;
            set => _subcontext.RequestAborted = value;
        }

        public override string TraceIdentifier
        {
            get => _subcontext.TraceIdentifier;
            set => _subcontext.TraceIdentifier = value;
        }

        public override ISession Session
        {
            get => _subcontext.Session;
            set => _subcontext.Session = value;
        }

        public override void Abort()
        {
            _subcontext.Abort();
        }
    }
}