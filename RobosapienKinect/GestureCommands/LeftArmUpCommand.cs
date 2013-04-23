using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class LeftArmUpCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.LeftForeArmStatus == ArmStatus.ArmUp) {
                return false;
            }

            double leftForearmAngle = GetAngle(joints[JointType.ShoulderLeft].AsVector3D(), joints[JointType.WristLeft].AsVector3D(), joints[JointType.ElbowLeft].AsVector3D());

            return leftForearmAngle <= Angles.FORE_ARM_UP;
        }

        public override void Execute() {
            RoboManagerInstance.LeftArmUp();
        }
    }
}