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
    using System.Collections.Generic;

    /// <summary>
    /// A collection of helper methods for <see cref="FieldItemResolutionStatus"/>.
    /// </summary>
    public static class FieldItemResolutionStatusExtensions
    {
        private static readonly Dictionary<FieldItemResolutionStatus, HashSet<FieldItemResolutionStatus>> CAN_BECOME_STATI;

        /// <summary>
        /// Initializes static members of the <see cref="FieldItemResolutionStatusExtensions"/> class.
        /// </summary>
        static FieldItemResolutionStatusExtensions()
        {
            CAN_BECOME_STATI = new Dictionary<FieldItemResolutionStatus, HashSet<FieldItemResolutionStatus>>();

            // become complete
            var canBecomeComplete = new HashSet<FieldItemResolutionStatus>();
            canBecomeComplete.Add(FieldItemResolutionStatus.NeedsChildResolution);
            canBecomeComplete.Add(FieldItemResolutionStatus.NotStarted);
            canBecomeComplete.Add(FieldItemResolutionStatus.ResultAssigned);
            CAN_BECOME_STATI.Add(FieldItemResolutionStatus.Complete, canBecomeComplete);

            // become canceled
            var canBecomeCanceled = new HashSet<FieldItemResolutionStatus>();
            canBecomeCanceled.Add(FieldItemResolutionStatus.NeedsChildResolution);
            canBecomeCanceled.Add(FieldItemResolutionStatus.NotStarted);
            canBecomeCanceled.Add(FieldItemResolutionStatus.Complete);
            canBecomeCanceled.Add(FieldItemResolutionStatus.ResultAssigned);
            CAN_BECOME_STATI.Add(FieldItemResolutionStatus.Canceled, canBecomeCanceled);

            // become failed
            var canBecomeFailed = new HashSet<FieldItemResolutionStatus>();
            canBecomeFailed.Add(FieldItemResolutionStatus.NeedsChildResolution);
            canBecomeFailed.Add(FieldItemResolutionStatus.NotStarted);
            canBecomeFailed.Add(FieldItemResolutionStatus.ResultAssigned);
            CAN_BECOME_STATI.Add(FieldItemResolutionStatus.Failed, canBecomeFailed);

            // become not started
            CAN_BECOME_STATI.Add(FieldItemResolutionStatus.NotStarted, new HashSet<FieldItemResolutionStatus>());

            // become "needs children"
            var canBecomeNeedsChildren = new HashSet<FieldItemResolutionStatus>();
            canBecomeNeedsChildren.Add(FieldItemResolutionStatus.NotStarted);
            canBecomeNeedsChildren.Add(FieldItemResolutionStatus.ResultAssigned);
            CAN_BECOME_STATI.Add(FieldItemResolutionStatus.NeedsChildResolution, canBecomeNeedsChildren);

            // become "result assigned"
            var canBecomeResultsAssigned = new HashSet<FieldItemResolutionStatus>();
            canBecomeResultsAssigned.Add(FieldItemResolutionStatus.NotStarted);
            CAN_BECOME_STATI.Add(FieldItemResolutionStatus.ResultAssigned, canBecomeResultsAssigned);

            // become "invalid"
            var canBecomeInvalid = new HashSet<FieldItemResolutionStatus>();
            canBecomeInvalid.Add(FieldItemResolutionStatus.NotStarted);
            canBecomeInvalid.Add(FieldItemResolutionStatus.ResultAssigned);
            canBecomeInvalid.Add(FieldItemResolutionStatus.NeedsChildResolution);
            canBecomeInvalid.Add(FieldItemResolutionStatus.Complete);
            CAN_BECOME_STATI.Add(FieldItemResolutionStatus.Invalid, canBecomeInvalid);
        }

        /// <summary>
        /// Determines if the given resolution status is one such taht the field results should
        /// be included in the final context output where the resolution occure.
        /// </summary>
        /// <param name="resolutionStatus">The resolution status.</param>
        /// <returns><c>true</c> if this status represents one to have its results included, <c>false</c> otherwise.</returns>
        public static bool IncludeInOutput(this FieldItemResolutionStatus resolutionStatus)
        {
            switch (resolutionStatus)
            {
                case FieldItemResolutionStatus.Complete:
                case FieldItemResolutionStatus.Failed:
                case FieldItemResolutionStatus.Invalid:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if the given resolution status is one such that, given the process chain, can be moved forward
        /// into the newly provided status.
        /// </summary>
        /// <param name="resolutionStatus">The current status.</param>
        /// <param name="newStatus">The new status.</param>
        /// <returns><c>true</c> if this instance can be advanced; otherwise, <c>false</c>.</returns>
        public static bool CanBeAdvancedTo(this FieldItemResolutionStatus resolutionStatus, FieldItemResolutionStatus newStatus)
        {
            return CAN_BECOME_STATI != null &&
                   CAN_BECOME_STATI.ContainsKey(newStatus) &&
                   CAN_BECOME_STATI[newStatus].Contains(resolutionStatus);
        }

        /// <summary>
        /// Determines whether a given resolution status indicates an unexpected error occured.
        /// </summary>
        /// <param name="resolutionStatus">The resolution status.</param>
        /// <returns><c>true</c> if the status indicates some type of error occured; otherwise, <c>false</c>.</returns>
        public static bool IndicatesAnError(this FieldItemResolutionStatus resolutionStatus)
        {
            return resolutionStatus == FieldItemResolutionStatus.Invalid || resolutionStatus == FieldItemResolutionStatus.Failed;
        }

        /// <summary>
        /// Determines whether a given resolution status is in a state that requires no additional processing.
        /// </summary>
        /// <param name="resolutionstatus">The resolutionstatus.</param>
        /// <returns><c>true</c> if the specified resolution status is finalized; otherwise, <c>false</c>.</returns>
        public static bool IsFinalized(this FieldItemResolutionStatus resolutionstatus)
        {
            switch (resolutionstatus)
            {
                case FieldItemResolutionStatus.Canceled:
                case FieldItemResolutionStatus.Failed:
                case FieldItemResolutionStatus.Invalid:
                    return true;

                default:
                    return false;
            }
        }
    }
}