﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.QueryPlans.Document.Parts.SuppliedValues
{
    using System;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Execution.QueryPlans.Document.Parts.Common;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts;
    using GraphQL.AspNet.Interfaces.Execution.QueryPlans.Document.Parts.Common;

    /// <summary>
    /// A base class providing common functionality for any "supplied value" to an argument
    /// or variable in a query document.
    /// </summary>
    [Serializable]
    internal abstract class DocumentSuppliedValue : DocumentPartBase, ISuppliedValueDocumentPart
    {
        protected DocumentSuppliedValue(IDocumentPart parentPart, SourceLocation location, string key = null)
            : base(parentPart, location)
        {
            this.Key = key?.Trim();
        }

        /// <inheritdoc />
        public abstract bool IsEqualTo(ISuppliedValueDocumentPart value);

        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.SuppliedValue;
    }
}