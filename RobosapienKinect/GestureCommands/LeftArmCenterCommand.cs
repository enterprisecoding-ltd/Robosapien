using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class LeftArmCenterCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.LeftArmStatus == ArmStatus.ArmMiddle) { return false; }

            var shoulderCenter = joints[JointType.ShoulderCenter].AsVector3D();

            var leftArmAngle = GetAngle(joints[JointType.ElbowLeft].AsVector3D() - shoulderCenter,
                                        joints[JointType.Spine].AsVector3D() - shoulderCenter,
                                        shoulderCenter);

            return leftArmAngle > Angles.ARM_IN && leftArmAngle < Angles.ARM_OUT;
        }

        public override void Execute() {
            if (RoboManagerInstance.LeftArmStatus == ArmStatus.ArmUp) {
                RoboManagerInstance.LeftArmIn();
            }
            else {
                RoboManagerInstance.LeftArmOut();
            }
        }
    }
}