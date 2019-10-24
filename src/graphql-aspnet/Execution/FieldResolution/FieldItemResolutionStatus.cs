// *************************************************************
// project:  graphql-aspnet
// --
// repo: https://github.com/graphql-aspnet
// docs: https://graphql-aspnet.github.io
// --
// License:  MIT
// *************************************************************
namespace GraphQL.AspNet.Execution.FieldResolution
{
    /// <summary>
    /// An enumeration of the various stati an data item can be in while its being processed by
    /// the runtime.
    /// </summary>
    public enum FieldItemResolutionStatus
    {
        /// <summary>
        /// The resolution pipeline for this specific instance has not yet begun
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// A resolver has completed its operation and a result value has been assigned to this instance. The
        /// result has not yet been validated or verified.
        /// </summary>
        ResultAssigned = 50,

        /// <summary>
        /// The assigned result has been evaluated and determined that any child field contexts that should
        /// be applied to this instance can and should be processed according to their own setup and that the
        /// result value of this instance can be used as an input value.
        /// </summary>
        NeedsChildResolution = 100,

        /// <summary>
        /// This instance has completed all operations successfully. Its result is final, no further changes can be made to it.
        /// </summary>
        Complete = 200,

        /// <summary>
        /// A field resolution operation failed to produce a result. The completion of this instance has been halted
        /// and no child contexts will be executed.
        /// </summary>
        Failed = 300,

        /// <summary>
        /// A field resolution operation was knowingly canceled (perhaps from a security violation or directive execution).
        /// This instance is considered to have "completed successfully"but will not perform any further
        /// operations and will be removed from any generated results.
        /// </summary>
        Canceled = 400,

        /// <summary>
        /// A field resolution operation completed successfully however, the returned result is not valid for this
        /// instance. Its result has been dropped from the operation.
        /// </summary>
        Invalid = 500,
    }
}