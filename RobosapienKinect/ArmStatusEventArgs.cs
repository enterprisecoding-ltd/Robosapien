using System;

namespace Com.Enterprisecoding.RobosapienKinect {
    internal class ArmStatusEventArgs : EventArgs {
        public readonly ArmStatus NewStatus;
        public readonly ArmStatus OldStatus;

        public ArmStatusEventArgs(ArmStatus oldStatus, ArmStatus newStatus) {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }
}