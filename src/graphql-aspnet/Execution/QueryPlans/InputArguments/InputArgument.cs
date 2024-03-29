﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.InputArguments
{
    using System;
    using System.Diagnostics;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.InputArguments;
    using GraphQL.AspNet.Interfaces.Schema;

    /// <summary>
    /// A single argument to a field (or directive) as its represented in a parsed query
    /// document. This represents the marrying of the field's defined argument in the schema
    /// and a supplied argument value.
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Name}")]
    public class InputArgument
    { /// <summary>
      /// Initializes a new instance of the <see cref="InputArgument" /> class.
      /// </summary>
      /// <param name="argument">The field argument defined in a schema.</param>
      /// <param name="value">The value representing this field argument as its defined in a query document.</param>
        public InputArgument(IGraphArgument argument, IInputValue value)
            : this(argument, value, SourceOrigin.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputArgument" /> class.
        /// </summary>
        /// <param name="argument">The field argument defined in a schema.</param>
        /// <param name="value">The value representing this field argument as its defined in a query document.</param>
        /// <param name="origin">The origin in a query document where the argument was generated.</param>
        public InputArgument(IGraphArgument argument, IInputValue value, SourceOrigin origin)
        {
            this.Argument = Validation.ThrowIfNullOrReturn(argument, nameof(argument));
            this.Value = Validation.ThrowIfNullOrReturn(value, nameof(value));
            this.Origin = origin;
        }

        /// <summary>
        /// Gets the formal name of argument on the field, in the schema.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.Argument.Name;

        /// <summary>
        /// Gets an object container that can resolve the value of this input argument
        /// at runtime. This value container may itself hold a pre-resolved static value or a
        /// reference to an input variable which it needs to extract from a variable collection.
        /// </summary>
        /// <value>The value.</value>
        public IInputValue Value { get; }

        /// <summary>
        /// Gets the underlying field that represents
        /// this argument on the target schema.
        /// </summary>
        /// <value>The argument.</value>
        public IGraphArgument Argument { get; }

        /// <summary>
        /// Gets the origin in a query document, if any, where this argument was
        /// created.
        /// </summary>
        /// <value>The origin location in a query document.</value>
        public SourceOrigin Origin { get; }
    }
}