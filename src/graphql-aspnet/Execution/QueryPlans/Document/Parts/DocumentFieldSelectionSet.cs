// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.PlanGeneration.Document.Parts
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using GraphQL.AspNet.Common.Source;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;
    using GraphQL.AspNet.Interfaces.PlanGeneration.Resolvables;
    using GraphQL.AspNet.Interfaces.TypeSystem;
    using GraphQL.AspNet.PlanGeneration.Document.Parts.Common;

    /// <summary>
    /// A collection of fields (from a <see cref="IGraphType"/>) that are requested by a user and defined
    /// on their query document. Selected fields are keyed by the return value (a.k.a. the field alias) requested by the user.
    /// </summary>
    [DebuggerDisplay("{Description}")]
    internal class DocumentFieldSelectionSet : DocumentPartBase, IFieldSelectionSetDocumentPart, IDocumentPart, IDecdendentDocumentPartSubscriber
    {
        private ExecutionFieldSet _executionSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFieldSelectionSet" /> class.
        /// </summary>
        /// <param name="parent">The parent document part that owns this set.</param>
        public DocumentFieldSelectionSet(IDocumentPart parent, SourceLocation location)
            : base(parent, location)
        {
            this.AssignGraphType(parent.GraphType);
            _executionSet = new ExecutionFieldSet(this);
        }

        /// <inheritdoc cref="IDecdendentDocumentPartSubscriber.OnDecendentPartAdded" />
        void IDecdendentDocumentPartSubscriber.OnDecendentPartAdded(IDocumentPart decendentPart, int relativeDepth)
        {
            _executionSet.UpdateSnapshot();
        }

        /// <inheritdoc />
        public override DocumentPartType PartType => DocumentPartType.FieldSelectionSet;

        /// <inheritdoc />
        public IExecutableFieldSelectionSet ExecutableFields => _executionSet;

        /// <inheritdoc />
        public override string Description
        {
            get
            {
                switch (this.Parent)
                {
                    case IFieldDocumentPart fd:

                        return $"FIELD SET for field {fd.Name}";

                    case IInlineFragmentDocumentPart iif:
                        return $"FIELD SET for inline fragment of {iif.Parent.Description}";

                    case INamedFragmentDocumentPart nfd:
                        return $"FIELD SET for fragment  {nfd.Name}";

                    case IOperationDocumentPart op:
                        var opname = op.Name?.Trim();
                        if (string.IsNullOrWhiteSpace(opname))
                            opname = "{anonymous}";
                        return $"FIELD SET for operation '{opname}'";

                    case IComplexSuppliedValueDocumentPart cs:
                        if (cs.Parent is IInputArgumentDocumentPart ia)
                            return $"FIELD SET for argument {ia.Name}";
                        else if (cs.Parent is IResolvableKeyedItem keyedItem)
                            return $"FIELD SET for complex value '{keyedItem.Key}'";
                        break;
                }

                return "FIELD SET for " + this.Parent.Description;
            }
        }
    }
}