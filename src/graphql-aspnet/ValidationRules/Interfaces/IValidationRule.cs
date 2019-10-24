// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.ValidationRules.Interfaces
{
    /// <summary>
    /// An interface describing a type of rule that may be executed in the rule processors.
    /// </summary>
    public interface IValidationRule
    {
        /// <summary>
        /// Gets the error code to associate with the broken rule.
        /// </summary>
        /// <value>The error code.</value>
        string ErrorCode { get; }

        /// <summary>
        /// Gets the rule number being validated in this instance (e.g. "X.Y.Z"), if any.
        /// </summary>
        /// <value>The rule number.</value>
        string RuleNumber { get; }

        /// <summary>
        /// Gets a url pointing to the rule definition in the graphql specification, if any.
        /// </summary>
        /// <value>The rule URL.</value>
        string ReferenceUrl { get; }
    }
}