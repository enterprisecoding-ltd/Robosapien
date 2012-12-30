using System;
using UsbUirt;

namespace Com.Enterprisecoding.RobosapienKinect {
    internal sealed class RoboManager {
        private static RoboManager instance;

        private ArmStatus leftForeArmStatus;
        private ArmStatus rightForeArmStatus;
        private ArmStatus leftArmStatus;
        private ArmStatus rightArmStatus;
        private bool followUp;
       
        public static RoboManager Instance {
            get {
                if (instance==null) {
                    instance = new RoboManager();
                }

                return instance;
            }
        }

        #region Events
        public event EventHandler<ArmStatusEventArgs> LeftForeArmStatusChanged;
        public event EventHandler<ArmStatusEventArgs> RightForeArmStatusChanged;
        public event EventHandler<ArmStatusEventArgs> LeftArmStatusChanged;
        public event EventHandler<ArmStatusEventArgs> RightArmStatusChanged;

        public event EventHandler<EventArgs> FollowUpChanged; 
        #endregion

        #region Status
        public ArmStatus LeftForeArmStatus {
            get { return leftForeArmStatus; }
            set {
                if (leftForeArmStatus == value) { return; }

                var oldStatus = leftForeArmStatus;
                leftForeArmStatus = value;

                if (LeftForeArmStatusChanged != null) {
                    LeftForeArmStatusChanged(this, new ArmStatusEventArgs(oldStatus, leftForeArmStatus));
                }
            }
        } 
        
        public ArmStatus RightForeArmStatus {
            get { return rightForeArmStatus; }
            set {
                if (rightForeArmStatus == value) { return; }

                var oldStatus = rightForeArmStatus;
                rightForeArmStatus = value;

                if (RightForeArmStatusChanged != null) {
                    RightForeArmStatusChanged(this, new ArmStatusEventArgs(oldStatus, rightForeArmStatus));
                }
            }
        } 

        public ArmStatus LeftArmStatus {
            get { return leftArmStatus; }
            set {
                if (leftArmStatus == value) { return; }

                var oldStatus = leftArmStatus;
                leftArmStatus = value;

                if (LeftArmStatusChanged != null) {
                    LeftArmStatusChanged(this, new ArmStatusEventArgs(oldStatus, leftArmStatus));
                }
            }
        } 

        public ArmStatus RightArmStatus {
            get { return rightArmStatus; }
            set {
                if (rightArmStatus == value) { return; }

                var oldStatus = rightArmStatus;
                rightArmStatus = value;

                if (RightArmStatusChanged!=null) {
                    RightArmStatusChanged(this, new ArmStatusEventArgs(oldStatus, rightArmStatus));
                }
            }
        } 
        #endregion

        public bool FollowUp {
            get { return followUp; }
            set {
                if (followUp == value) { return; }

                followUp = value;

                if (FollowUpChanged!=null) {
                    FollowUpChanged(this, new EventArgs());
                }
            }
        }

        private RoboManager() {
            Reset();
        }

        public void Reset() {
            LeftForeArmStatus = ArmStatus.ArmDown;
            RightForeArmStatus = ArmStatus.ArmDown;

            LeftArmStatus = ArmStatus.ArmDown;
            RightArmStatus = ArmStatus.ArmDown;
        }
        
        #region Left Arm
        public void LeftArmDown() {
            if (LeftForeArmStatus == ArmStatus.ArmDown) { return; }
           
            LeftForeArmStatus = LeftForeArmStatus == ArmStatus.ArmMiddle ? ArmStatus.ArmDown : ArmStatus.ArmMiddle;
        }

        public void LeftArmUp() {
            if (LeftForeArmStatus == ArmStatus.ArmUp) { return; }
           
            LeftForeArmStatus = LeftForeArmStatus == ArmStatus.ArmMiddle ? ArmStatus.ArmUp : ArmStatus.ArmMiddle;
        }

        public void LeftArmIn() {
            if (LeftArmStatus == ArmStatus.ArmDown) { return; }

            LeftArmStatus = LeftArmStatus == ArmStatus.ArmMiddle ? ArmStatus.ArmDown : ArmStatus.ArmMiddle;
        }

        public void LeftArmOut() {
            if (LeftArmStatus == ArmStatus.ArmUp) { return; }

            LeftArmStatus = LeftArmStatus == ArmStatus.ArmMiddle ? ArmStatus.ArmUp : ArmStatus.ArmMiddle;
        } 
        #endregion

        #region Right Arm
        public void RightArmDown() {
            if (RightForeArmStatus == ArmStatus.ArmDown) { return; }

            RightForeArmStatus = RightForeArmStatus == ArmStatus.ArmMiddle ? ArmStatus.ArmDown : ArmStatus.ArmMiddle;
        }

        public void RightArmUp() {
            if (RightForeArmStatus == ArmStatus.ArmUp) { return; }

            RightForeArmStatus = RightForeArmStatus == ArmStatus.ArmMiddle ? ArmStatus.ArmUp : ArmStatus.ArmMiddle;
        }

        public void RightArmIn() {
            if (RightArmStatus == ArmStatus.ArmDown) { return; }

            RightArmStatus = RightArmStatus == ArmStatus.ArmMiddle ? ArmStatus.ArmDown : ArmStatus.ArmMiddle;
        }

        public void RightArmOut() {
            if (RightArmStatus == ArmStatus.ArmUp) { return; }

            RightArmStatus = RightArmStatus == ArmStatus.ArmMiddle ? ArmStatus.ArmUp : ArmStatus.ArmMiddle;
        } 
        #endregion

        #region Tilt Body
        public void TiltBodyRight() {
            throw new NotImplementedException();
        }

        public void TiltBodyLeft() {
            throw new NotImplementedException();
        } 
        #endregion

        #region Lean Body
        public void LeanForward() {
            throw new NotImplementedException();
        }

        public void LeanBackward() {
            throw new NotImplementedException();
        } 
        #endregion
    }
}
