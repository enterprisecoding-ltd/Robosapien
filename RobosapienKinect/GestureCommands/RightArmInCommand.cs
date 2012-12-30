using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class RightArmInCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.RightArmStatus == ArmStatus.ArmDown) { return false; }

            var shoulderCenter = joints[JointType.ShoulderCenter].AsVector3D();

            var rightArmAngle = GetAngle(joints[JointType.ElbowRight].AsVector3D() - shoulderCenter,
                                        joints[JointType.Spine].AsVector3D() - shoulderCenter,
                                        shoulderCenter);

            return rightArmAngle <= Angles.ARM_IN;
        }

        public override void Execute() {
            RoboManagerInstance.RightArmIn();
        }
    }
}