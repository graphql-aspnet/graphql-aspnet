// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Tests.Logging.LoggerTestData
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ValidatableObject
    {
        private int _age;

        [Range(0, 35)]
        public int Age
        {
            get
            {
                if (_age == 66)
                    throw new InvalidOperationException("Can't be age 66!");

                return _age;
            }

            set
            {
                _age = value;
            }
        }
    }
}