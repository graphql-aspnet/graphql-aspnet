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
    /// A collection of helper methods for <see cref="FieldDataItemResolutionStatus"/>.
    /// </summary>
    internal static class FieldDataItemResolutionStatusExtensions
    {
        private static readonly Dictionary<FieldDataItemResolutionStatus, HashSet<FieldDataItemResolutionStatus>> CAN_BECOME_STATI;

        /// <summary>
        /// Initializes static members of the <see cref="FieldDataItemResolutionStatusExtensions"/> class.
        /// </summary>
        static FieldDataItemResolutionStatusExtensions()
        {
            CAN_BECOME_STATI = new Dictionary<FieldDataItemResolutionStatus, HashSet<FieldDataItemResolutionStatus>>();

            // become complete
            var canBecomeComplete = new HashSet<FieldDataItemResolutionStatus>();
            canBecomeComplete.Add(FieldDataItemResolutionStatus.NeedsChildResolution);
            canBecomeComplete.Add(FieldDataItemResolutionStatus.NotStarted);
            canBecomeComplete.Add(FieldDataItemResolutionStatus.ResultAssigned);
            CAN_BECOME_STATI.Add(FieldDataItemResolutionStatus.Complete, canBecomeComplete);

            // become canceled
            var canBecomeCanceled = new HashSet<FieldDataItemResolutionStatus>();
            canBecomeCanceled.Add(FieldDataItemResolutionStatus.NeedsChildResolution);
            canBecomeCanceled.Add(FieldDataItemResolutionStatus.NotStarted);
            canBecomeCanceled.Add(FieldDataItemResolutionStatus.Complete);
            canBecomeCanceled.Add(FieldDataItemResolutionStatus.ResultAssigned);
            CAN_BECOME_STATI.Add(FieldDataItemResolutionStatus.Canceled, canBecomeCanceled);

            // become failed
            var canBecomeFailed = new HashSet<FieldDataItemResolutionStatus>();
            canBecomeFailed.Add(FieldDataItemResolutionStatus.NeedsChildResolution);
            canBecomeFailed.Add(FieldDataItemResolutionStatus.NotStarted);
            canBecomeFailed.Add(FieldDataItemResolutionStatus.ResultAssigned);
            CAN_BECOME_STATI.Add(FieldDataItemResolutionStatus.Failed, canBecomeFailed);

            // become not started
            CAN_BECOME_STATI.Add(FieldDataItemResolutionStatus.NotStarted, new HashSet<FieldDataItemResolutionStatus>());

            // become "needs children"
            var canBecomeNeedsChildren = new HashSet<FieldDataItemResolutionStatus>();
            canBecomeNeedsChildren.Add(FieldDataItemResolutionStatus.NotStarted);
            canBecomeNeedsChildren.Add(FieldDataItemResolutionStatus.ResultAssigned);
            CAN_BECOME_STATI.Add(FieldDataItemResolutionStatus.NeedsChildResolution, canBecomeNeedsChildren);

            // become "result assigned"
            var canBecomeResultsAssigned = new HashSet<FieldDataItemResolutionStatus>();
            canBecomeResultsAssigned.Add(FieldDataItemResolutionStatus.NotStarted);
            CAN_BECOME_STATI.Add(FieldDataItemResolutionStatus.ResultAssigned, canBecomeResultsAssigned);

            // become "invalid"
            var canBecomeInvalid = new HashSet<FieldDataItemResolutionStatus>();
            canBecomeInvalid.Add(FieldDataItemResolutionStatus.NotStarted);
            canBecomeInvalid.Add(FieldDataItemResolutionStatus.ResultAssigned);
            canBecomeInvalid.Add(FieldDataItemResolutionStatus.NeedsChildResolution);
            canBecomeInvalid.Add(FieldDataItemResolutionStatus.Complete);
            CAN_BECOME_STATI.Add(FieldDataItemResolutionStatus.Invalid, canBecomeInvalid);
        }

        /// <summary>
        /// Determines if the given resolution status is one such taht the field results should
        /// be included in the final context output where the resolution occure.
        /// </summary>
        /// <param name="resolutionStatus">The resolution status.</param>
        /// <returns><c>true</c> if this status represents one to have its results included, <c>false</c> otherwise.</returns>
        public static bool IncludeInOutput(this FieldDataItemResolutionStatus resolutionStatus)
        {
            switch (resolutionStatus)
            {
                case FieldDataItemResolutionStatus.Complete:
                case FieldDataItemResolutionStatus.Failed:
                case FieldDataItemResolutionStatus.Invalid:
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
        public static bool CanBeAdvancedTo(this FieldDataItemResolutionStatus resolutionStatus, FieldDataItemResolutionStatus newStatus)
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
        public static bool IndicatesAnError(this FieldDataItemResolutionStatus resolutionStatus)
        {
            return resolutionStatus == FieldDataItemResolutionStatus.Invalid || resolutionStatus == FieldDataItemResolutionStatus.Failed;
        }

        /// <summary>
        /// Determines whether a given resolution status is in a state that requires no additional processing.
        /// </summary>
        /// <param name="resolutionstatus">The resolutionstatus.</param>
        /// <returns><c>true</c> if the specified resolution status is finalized; otherwise, <c>false</c>.</returns>
        public static bool IsFinalized(this FieldDataItemResolutionStatus resolutionstatus)
        {
            switch (resolutionstatus)
            {
                case FieldDataItemResolutionStatus.Canceled:
                case FieldDataItemResolutionStatus.Failed:
                case FieldDataItemResolutionStatus.Invalid:
                    return true;

                default:
                    return false;
            }
        }
    }
}