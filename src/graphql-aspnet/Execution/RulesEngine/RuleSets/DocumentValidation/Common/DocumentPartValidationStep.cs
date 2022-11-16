// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.RulesEngine.RuleSets.DocumentValidation.Common
{
    using System;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Execution.RulesEngine.Interfaces;

    /// <summary>
    /// A base step with commmon logic for all document validation steps.
    /// </summary>
    internal abstract class DocumentPartValidationStep : IRuleStep<DocumentValidationContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentPartValidationStep"/> class.
        /// </summary>
        protected DocumentPartValidationStep()
        {
            this.RuleId = Guid.NewGuid();
        }

        /// <summary>
        /// Attempts to retrieve the metadata object from the context for this specific rule.
        /// If the metadata object does not already exist the <paramref name="dataFactory"/>
        /// will be invoked to create it and store it before returning it.
        /// </summary>
        /// <remarks>
        /// An exception will be thrown if a metadata object exists for this rule
        /// but is not castable to the provided <typeparamref name="TDataType"/>.</remarks>
        /// <typeparam name="TDataType">The type of the metadata object.</typeparam>
        /// <param name="context">The context to inspect.</param>
        /// <param name="dataFactory">The data factory used for creating new values.</param>
        /// <returns>The metadata object for this rule on the supplied context.</returns>
        protected TDataType GetOrAddMetaData<TDataType>(DocumentValidationContext context, Func<TDataType> dataFactory)
            where TDataType : class
        {
            if (context.RuleMetaData.ContainsKey(this.RuleId))
            {
                var data = context.RuleMetaData[this.RuleId] as TDataType;
                if (data == null)
                {
                    throw new InvalidCastException(
                        $"Unable to cast the metadata object to the provided " +
                        $"type of {typeof(TDataType).FriendlyName()}");
                }

                return data;
            }

            var newData = dataFactory();
            context.RuleMetaData.Add(this.RuleId, newData);
            return newData;
        }

        /// <summary>
        /// Registers a validation error with the local message collection as a critical error. The validation
        /// message will automatically be appended with the appropriate message extensions to reference the error being validated.
        /// </summary>
        /// <param name="context">The validation context in scope.</param>
        /// <param name="message">The error message.</param>
        protected virtual void ValidationError(DocumentValidationContext context, string message)
        {
            this.ValidationError(context, context.ActivePart.SourceLocation, message);
        }

        /// <summary>
        /// Registers a validation error with the local message collection as a critical error. The validation
        /// message will automatically be appended with the appropriate message extensions to reference the error being validated.
        /// </summary>
        /// <param name="context">The validation context in scope.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="message">The error message.</param>
        protected virtual void ValidationError(DocumentValidationContext context, SourceLocation sourceLocation, string message)
        {
            context.Messages.Critical(
                message,
                this.ErrorCode,
                sourceLocation.AsOrigin());
        }

        /// <inheritdoc />
        public virtual bool ShouldAllowChildContextsToExecute(DocumentValidationContext context)
        {
            return true;
        }

        /// <inheritdoc />
        public abstract bool ShouldExecute(DocumentValidationContext context);

        /// <inheritdoc />
        public abstract bool Execute(DocumentValidationContext context);

        /// <summary>
        /// Gets the error code to associate with any validation messages recorded.
        /// </summary>
        /// <value>The error code.</value>
        public virtual string ErrorCode => Constants.ErrorCodes.INVALID_DOCUMENT;

        /// <inheritdoc />
        public Guid RuleId { get; }
    }
}