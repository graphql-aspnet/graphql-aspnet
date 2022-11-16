﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Defaults.TypeMakers.TestData
{
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Configuration;

    [GraphType(FieldDeclarationRequirements = TemplateDeclarationRequirements.RequireMethodAndProperties)]
    public class TypeWithUndeclaredFieldsWithOverride
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