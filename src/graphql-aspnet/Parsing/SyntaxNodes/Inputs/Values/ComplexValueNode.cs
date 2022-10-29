﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Parsing.SyntaxNodes.Inputs.Values
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;

    /// <summary>
    /// A representation of a complex piece of data (not a scalar value and not an Enum value) passed as input
    /// to the query.
    /// </summary>
    [DebuggerDisplay("ComplexValue (Children = {Children.Count})")]
    public class ComplexValueNode : InputValueNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexValueNode" /> class.
        /// </summary>
        /// <param name="location">The location.</param>
        public ComplexValueNode(SourceLocation location)
            : base(location, ReadOnlyMemory<char>.Empty)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"CV";
        }
    }
}