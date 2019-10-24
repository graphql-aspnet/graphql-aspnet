// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************

namespace GraphQL.AspNet.Execution
{
    using System.Collections.Generic;

    /// <summary>
    /// An enumeration representing how this extension field is handled by the runtime.
    /// </summary>
    public enum FieldResolutionMode
    {
        /// <summary>
        /// <para>This field will be called once per instance of each source object and the results of the call will be used as the result
        /// of the field. This is the default mode for most field resolutions.</para>
        ///
        /// <para>If this field defines a parameter with the same type as the source data object, that parameter will be
        /// automatically supplied the source data in question for reference. Any other parameters will be added as inputs to the field
        /// in the object graph.</para>
        /// </summary>
        PerSourceItem = 0,

        /// <summary>
        /// <para>For a collection of items this field is called once and the results are split per item of the collection.
        /// The return type of this method should be dictionary (keyed on source object).</para>
        ///
        /// <para>This method should take, at a minimum, an <see cref="IEnumerable{T}"/> of the source data type.
        /// Any other parameters will be added as inputs to the field in the object graph.</para>
        ///
        /// <para>Use this mode to solve the common N+1 problem.</para>
        /// </summary>
        Batch = 1,
    }
}