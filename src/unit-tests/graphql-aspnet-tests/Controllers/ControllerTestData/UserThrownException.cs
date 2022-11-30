// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Controllers.ControllerTestData
{
    using System;

    public class UserThrownException : Exception
    {
        public UserThrownException()
        : base("Test message from user exception")
        {
        }
    }
}