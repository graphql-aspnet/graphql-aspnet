namespace GraphQL.AspNet.PlanGeneration.Document
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts;
    using GraphQL.AspNet.Interfaces.PlanGeneration.DocumentParts.Common;

    /// <summary>
    /// A node representing a field or set of fields within an <see cref="ExecutionFieldSet"/>.
    /// </summary>
    internal class ExecutableFieldNode
    {
        public IResolvableDocumentPart Owner { get; set; }

        public List<IDocumentPart>  { get; set; }


        /// <summary>
        /// Gets the total number of actual fields represented by this node and its children.
        /// This number includes all the first order children of <see cref="Owner"/>
        /// as well as its children etc.
        /// </summary>
        /// <value>The count of fields of this node and deeper.</value>
/        public int Count { get; private set; }
    }
}
