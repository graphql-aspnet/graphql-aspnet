﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.Parsing.SyntaxNodes
{
    using System;

    /// <summary>
    /// An enumeration for defining what type of scalar is being represented
    /// spec: https://graphql.github.io/graphql-spec/October2021/#sec-Scalars.
    /// </summary>
    [Flags]
    public enum ScalarValueType : byte
    {
        Unknown = 0,
        Number = 1,
        String = 2,
        Boolean = 4,
        StringOrNumber = Number | String,
    }
}