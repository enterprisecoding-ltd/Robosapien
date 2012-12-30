using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class LeftArmDownCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.LeftForeArmStatus == ArmStatus.ArmDown) { return false; }

            var leftForearmAngle = GetAngle(joints[JointType.ShoulderLeft].AsVector3D(), joints[JointType.WristLeft].AsVector3D(), joints[JointType.ElbowLeft].AsVector3D());

            return leftForearmAngle >= Angles.FORE_ARM_DOWN;
        }

        public override void Execute() {
            RoboManagerInstance.LeftArmDown();
        }
    }
}