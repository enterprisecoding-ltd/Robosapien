using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class LeftArmInCommand: GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.LeftArmStatus == ArmStatus.ArmDown) { return false; }

            var shoulderCenter = joints[JointType.ShoulderCenter].AsVector3D();

            var leftArmAngle = GetAngle(joints[JointType.ElbowLeft].AsVector3D() - shoulderCenter,
                                        joints[JointType.Spine].AsVector3D() - shoulderCenter,
                                        shoulderCenter);

            return leftArmAngle <= Angles.ARM_IN;
        }

        public override void Execute() {
            RoboManagerInstance.LeftArmIn();
        }
    }
}