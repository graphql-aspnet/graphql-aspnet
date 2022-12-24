﻿// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution.FieldResolution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using GraphQL.AspNet.Common;
    using GraphQL.AspNet.Common.Extensions;
    using GraphQL.AspNet.Execution.Exceptions;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Execution.Source;
    using GraphQL.AspNet.Execution.Response;
    using GraphQL.AspNet.Interfaces.Execution.Response;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Schemas;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using GraphQL.AspNet.Internal;

    /// <summary>
    /// An ecapsulation of a piece of real data supplied to, or resolved from, a graph field.
    /// </summary>
    [DebuggerDisplay("{Path.ToString()} (Status = {Status})")]
    public class FieldDataItem
    {
        private List<FieldDataItem> _childListItems;
        private List<FieldDataItem> _childFields;
        private bool _resultsDiscarded;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldDataItem" /> class.
        /// </summary>
        /// <param name="context">The context where the <paramref name="sourceData"/>
        /// is being processed.</param>
        /// <param name="sourceData">The source data item being wrapped.</param>
        /// <param name="path">The path to the data item in the query.</param>
        public FieldDataItem(IGraphFieldInvocationContext context, object sourceData, SourcePath path)
        {
            Validation.ThrowIfNull(path, nameof(path));
            this.SourceData = sourceData;
            this.FieldContext = Validation.ThrowIfNullOrReturn(context, nameof(context));
            this.Status = FieldDataItemResolutionStatus.NotStarted;
            this.Origin = new SourceOrigin(context.Location, path);
            this.TypeExpression = context.Field.TypeExpression;
            this.Schema = Validation.ThrowIfNullOrReturn(context.Schema, nameof(context.Schema));

            if (context.Field.Mode == FieldResolutionMode.Batch)
                _expectedChildFieldCount = 1;
            else
                _expectedChildFieldCount = context.ChildContexts.Count;
        }

        /// <summary>
        /// Adds a new field context item as a child of this field.  Throws an exception
        /// if this data item contains list items.
        /// </summary>
        /// <param name="childInvocationContext">The child invocation context.</param>
        /// <returns>GraphDataItem.</returns>
        public FieldDataItem AddChildField(IGraphFieldInvocationContext childInvocationContext)
        {
            if (this.IsListField)
            {
                throw new GraphExecutionException(
                    $"The field {this.FieldContext.Field.Route.Path} represents " +
                    "a list of items, a child field context cannot be directly added to it.");
            }

            _childFields = _childFields ?? new List<FieldDataItem>(_expectedChildFieldCount);

            var path = this.Origin.Path.Clone();
            path.AddFieldName(childInvocationContext.Field.Name);

            var childFieldItem = new FieldDataItem(childInvocationContext, this.ResultData, path);
            _childFields.Add(childFieldItem);
            return childFieldItem;
        }

        /// <summary>
        /// Adds a new field context item as a child of this field.  Throws an exception
        /// if this data item contains list items.
        /// </summary>
        /// <param name="childListItem">The child list item.</param>
        public void AddListItem(FieldDataItem childListItem)
        {
            if (!this.IsListField)
            {
                throw new GraphExecutionException(
                    $"The field {this.FieldContext.Field.Route.Path} represents " +
                    "a collection of fields, a child list item cannot be directly added to it.");
            }

            _childListItems = _childListItems ?? new List<FieldDataItem>();
            _childListItems.Add(childListItem);
        }

        /// <summary>
        /// Copies the state of this instance into the provided instance. This is a direct replacement of the state of the
        /// target item.
        /// </summary>
        /// <param name="item">The item to copy the state to.</param>
        public void CopyTo(FieldDataItem item)
        {
            if (item == null)
                return;

            item.Status = this.Status;
            item.TypeExpression = this.TypeExpression;
            item.ResultData = this.ResultData;
            item._resultsDiscarded = this._resultsDiscarded;
            item._childFields = this._childFields == null ? null : new List<FieldDataItem>(this._childFields);
            item._childListItems = this._childListItems == null ? null : new List<FieldDataItem>(this._childListItems);
        }

        /// <summary>
        /// If this instance represnets a list of items this method extracts all the leaf items (those that are not themselves lists) and returns
        /// them as an enumerable.  If this instance is not a list the instance itself is returned as an enumerable.
        /// </summary>
        /// <returns>System.Collections.Generic.IEnumerable&lt;GraphQL.AspNet.Execution.FieldResolution.GraphDataItem&gt;.</returns>
        internal IEnumerable<FieldDataItem> FlattenListItemTree()
        {
            if (!this.IsListField)
            {
                yield return this;
            }
            else if (_childListItems != null)
            {
                foreach (var leaf in _childListItems.SelectMany(x => x.FlattenListItemTree()))
                {
                    yield return leaf;
                }
            }
        }

        /// <summary>
        /// Advances this instance such that any child items present on it can be processed according to their
        /// own logic.
        /// </summary>
        public void RequireChildResolution()
        {
            this.SetStatus(FieldDataItemResolutionStatus.NeedsChildResolution);
        }

        /// <summary>
        /// Cancels processing of this instance ensuring its is not included in a results tree
        /// and that no children are executed underneath it.
        /// </summary>
        public void Cancel()
        {
            this.SetStatus(FieldDataItemResolutionStatus.Canceled);
        }

        /// <summary>
        /// Completes this instance disallowing any further changes to its data and marking this
        /// instance as not completing successfully.
        /// </summary>
        public void Fail()
        {
            this.SetStatus(FieldDataItemResolutionStatus.Failed);
        }

        /// <summary>
        /// Completes this instance disallowing any further changes to its data.
        /// </summary>
        public void Complete()
        {
            this.SetStatus(FieldDataItemResolutionStatus.Complete);
        }

        /// <summary>
        /// Completes this instance indicating that the result it was assigned
        /// is not valid in context for the field it represents. Processing is stopped
        /// and the value is ultimately discarded.
        /// </summary>
        public void InvalidateResult()
        {
            _resultsDiscarded = true;
            this.SetStatus(FieldDataItemResolutionStatus.Invalid);
        }

        /// <summary>
        /// Sets the status of this data item if/when allowed.
        /// </summary>
        /// <param name="newStatus">The new status.</param>
        protected void SetStatus(FieldDataItemResolutionStatus newStatus)
        {
            if (this.Status.CanBeAdvancedTo(newStatus))
                this.Status = newStatus;
        }

        /// <summary>
        /// Updates this instance with a piece of data recieved from the completion of a user resolver function.
        /// This method performs no validation or resolution of the data, it simply assigns it as a value that was recieved
        /// from a resolver function and attaches it to this instance.
        /// </summary>
        /// <param name="data">The data.</param>
        public virtual void AssignResult(object data)
        {
            _childListItems = null;
            _childFields = null;

            this.ResultData = data;
            this.SetStatus(FieldDataItemResolutionStatus.ResultAssigned);

            // initialize the children of this instance from the assigned result
            if (this.IsListField)
            {
                if (this.ResultData == null)
                    return;

                if (!GraphValidation.IsValidListType(this.ResultData.GetType()))
                    return;

                if (!(this.ResultData is IEnumerable arraySource))
                    return;

                _childListItems = new List<FieldDataItem>();

                // this instances's type expression is a list (or else we wouldnt be rendering out children)
                // strip out that outer most list component
                // to represent what each element of said list should be so that the rule
                // processor can validate the item as its own entity
                var childTypeExpression = this.TypeExpression.UnWrapExpression(MetaGraphTypes.IsList);

                var path = this.Path.Clone();
                int index = 0;

                foreach (var item in arraySource)
                {
                    var indexedPath = path.Clone();
                    indexedPath.AddArrayIndex(index++);
                    var childItem = new FieldDataItem(this.FieldContext, item, indexedPath)
                    {
                        TypeExpression = childTypeExpression.Clone(),
                    };

                    this.AddListItem(childItem);

                    // child items are immediately resolved
                    // using the enumerated source data from the resolver that supplied data
                    // to this parent item.
                    // that is to say that no child resolver is needed to be processed to
                    // retrieve data for each child individually.
                    childItem.AssignResult(item);
                    childItem.SetStatus(this.Status);
                }
            }
        }

        /// <summary>
        /// Gets the original source data reference, untouched, as it was supplied to the executor.
        /// </summary>
        /// <value>The original source data.</value>
        public object SourceData { get; }

        /// <summary>
        /// Gets the data assigned to this data item after the resolution of a user generated resolver.
        /// </summary>
        /// <value>The result data.</value>
        public object ResultData { get; private set; }

        /// <summary>
        /// Gets the conanical path to this item.
        /// </summary>
        /// <value>The path.</value>
        public SourcePath Path => this.Origin.Path;

        /// <summary>
        /// Gets the field where this source item is being processed.
        /// </summary>
        /// <value>The field.</value>
        public IGraphFieldInvocationContext FieldContext { get; }

        /// <summary>
        /// Gets the name of this data item as it would appear in the results tree.
        /// </summary>
        /// <value>The name.</value>
        public string Name => this.FieldContext.Name;

        /// <summary>
        /// Gets the status of processing for this source item.
        /// </summary>
        /// <value>The status.</value>
        public FieldDataItemResolutionStatus Status { get; private set; }

        /// <summary>
        /// Gets the full origin describing this item's location in the graph tree.
        /// </summary>
        /// <value>The origin.</value>
        public SourceOrigin Origin { get; }

        /// <summary>
        /// Gets the type expression representing this data item.
        /// </summary>
        /// <value>The type expression.</value>
        public GraphTypeExpression TypeExpression { get; private set; }

        /// <summary>
        /// Gets the schema on which this data item is based.
        /// </summary>
        /// <value>The schema.</value>
        public ISchema Schema { get; }

        private readonly int _expectedChildFieldCount;

        /// <summary>
        /// Gets the list items of this instance that were generated from resolved data, if any.
        /// </summary>
        /// <value>The children.</value>
        public IList<FieldDataItem> ListItems => _childListItems;

        /// <summary>
        /// Gets the fields generated from this instance.
        /// </summary>
        /// <value>The fields.</value>
        public IList<FieldDataItem> Fields => _childFields;

        /// <summary>
        /// Gets the collective set of child <see cref="FieldDataItem"/> generated
        /// by this instance be that tracked down stream fields or a collection of list items.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<FieldDataItem> Children
        {
            get
            {
                if (_childListItems != null)
                    return _childListItems;
                else if (_childFields != null)
                    return _childFields;

                return Enumerable.Empty<FieldDataItem>();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is represents a field that should return a list of items rather than a single object.
        /// </summary>
        /// <value><c>true</c> if this instance is list field; otherwise, <c>false</c>.</value>
        public virtual bool IsListField => this.TypeExpression.IsListOfItems;

        /// <summary>
        /// Generates the result for this data item.  Will correctly populate a list or key/value pait object
        /// according to the rules of the field this data item represents.
        /// </summary>
        /// <param name="result">The result object that was generated.</param>
        /// <returns><c>true</c> if the result was generated and should be included in any up stream responses. <c>false</c> if this result should be ignored
        /// and not included.</returns>
        public bool GenerateResult(out IQueryResponseItem result)
        {
            result = null;

            // no data resolved? no children can exist
            // return indicating if the "null" is acceptable or not.
            if (this.ResultData == null || _resultsDiscarded)
            {
                return this.Status.IncludeInOutput();
            }

            // total fail, womp womp
            if (!this.Status.IncludeInOutput())
                return false;

            // leafs have nothing underneath  them, the resolved data IS the item value.
            if (this.FieldContext.Field.IsLeaf)
            {
                // List<SomeScalar> and List<SomeEnum> are leafs since there is no further
                // resolution to the data but its still must be projected
                // as a list response item when rendered
                // ----
                return this.IsListField
                    ? this.GenerateListItemResult(out result)
                    : this.GenerateSingleValueResult(out result);
            }

            bool includeResult;
            if (_childFields != null)
            {
                includeResult = this.GenerateFieldListResult(out result);
            }
            else if (_childListItems != null)
            {
                includeResult = this.GenerateListItemResult(out result);
            }
            else
            {
                // no fields were resolved and no children containers were assigned
                // drop the item from the response. This can occur if a resolver returns
                // an object that is not scoped to the current query such as returning
                // Droids and Humans but the query is just a fragment spread for Humans
                // the droids would be dropped as they were not requested
                includeResult = false;
                result = null;
            }

            return includeResult;
        }

        /// <summary>
        /// Generates the single value result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns><c>true</c> if the leaf value was successfully rendered, <c>false</c> otherwise.</returns>
        private bool GenerateSingleValueResult(out IQueryResponseItem result)
        {
            result = null;
            var resultData = this.ResultData;
            if (resultData != null)
            {
                var type = resultData.GetType();
                var graphType = this.Schema.KnownTypes.FindGraphType(type);
                if (graphType is IScalarGraphType sgt)
                {
                    resultData = sgt.Serialize(resultData);
                }
            }

            result = new ResponseSingleValue(resultData);
            return true;
        }

        /// <summary>
        /// Generates a result representing the list items of this instance. This result
        /// is built in a manner that can be easily serialized.
        /// </summary>
        /// <param name="result">The result that was generated.</param>
        /// <returns><c>true</c> if the result that was generated is valid and should
        /// be included in a final result, <c>false</c> otherwise.</returns>
        private bool GenerateListItemResult(out IQueryResponseItem result)
        {
            result = null;
            var includeResult = false;

            // this instance represents an array of children
            // create an output array of the generated data for each child
            if (_childListItems.Count == 0)
            {
                result = new ResponseList();
                includeResult = true;
            }
            else
            {
                var list = new ResponseList();
                foreach (var child in _childListItems)
                {
                    var includeChildResult = child.GenerateResult(out var childResult);
                    includeResult = includeResult || includeChildResult;
                    if (includeChildResult)
                        list.Add(childResult);
                }

                if (includeResult)
                    result = list;
            }

            return includeResult;
        }

        /// <summary>
        /// Generates a result representing the individual child fields of this instance. This result
        /// is built in a manner that can be easily serialized.
        /// </summary>
        /// <param name="result">The result that was generated.</param>
        /// <returns><c>true</c> if the result that was generated is valid and should
        /// be included in a final result, <c>false</c> otherwise.</returns>
        private bool GenerateFieldListResult(out IQueryResponseItem result)
        {
            result = null;
            var includeResult = false;

            // this instance represents a set of key/value pair fields
            // create a dictionary of those kvps as the result
            var fieldSet = new ResponseFieldSet();

            for (var i = 0; i < _childFields.Count; i++)
            {
                var field = _childFields[i];
                var includeChildResult = field.GenerateResult(out var childResult);
                includeResult = includeResult || includeChildResult;
                if (includeChildResult)
                {
                    if (fieldSet.Fields.ContainsKey(field.FieldContext.Name))
                    {
                        throw new GraphExecutionException(
                            $"Duplicate field name. The field '{field.Name}'  at '{this.Origin.Path.ToDotString()}' was resolved " +
                            "more than once for a source object, unable to generate a valid output. " +
                            $"Field collections require unique names. An attempt was made to add the field '{field.Name}', " +
                            $"for target type '{field.FieldContext.ExpectedSourceType?.FriendlyName() ?? "-all-"}' when the field " +
                            "name was already present in the output dictionary.",
                            this.Origin,
                            new InvalidOperationException(
                                $"The source object '{this.SourceData}' successfully resolved a field name of '{field.Name}' more than once when it shouldn't. This may occur if a source " +
                                "object type is referenced to to multiple target graph types in fragment references. Ensure that your source data uniquely maps to one fragment per field collection " +
                                "or that the fragments do not share property names."));
                    }

                    fieldSet.Add(field.FieldContext.Name, childResult);
                }
            }

            if (includeResult)
                result = fieldSet;

            return includeResult;
        }
    }
}