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
    /// <summary>
    /// An enumeration of all the types of nodes that can exist within
    /// an abstract syntax tree representing a query.
    /// </summary>
    internal enum SyntaxNodeType : byte
    {
        Empty = 0,

        Document,
        Operation,
        VariableCollection,
        Variable,

        Directive,

        NamedFragment,
        InlineFragment,
        FragmentSpread,

        FieldCollection,
        Field,

        InputItemCollection,
        InputItem,
        InputValue,

        VariableValue,
        ComplexValue,
        EnumValue,
        ScalarValue,
        ListValue,
        NullValue,
    }
}