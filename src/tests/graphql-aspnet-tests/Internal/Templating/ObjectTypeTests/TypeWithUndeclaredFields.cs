// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Internal.Templating.ObjectTypeTests
{
    using GraphQL.AspNet.Attributes;

    public class TypeWithUndeclaredFields
    {
        [GraphField]
        public string DeclaredMethod()
        {
            return string.Empty;
        }

        public string UndeclaredMethod()
        {
            return string.Empty;
        }

        [GraphField]
        public string DeclaredProperty
        {
            get
            {
                return string.Empty;
            }

            set
            {
            }
        }

        public string UndeclaredProperty
        {
            get
            {
                return string.Empty;
            }

            set
            {
            }
        }
    }
}