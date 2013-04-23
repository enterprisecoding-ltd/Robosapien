using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class LeftArmMiddleCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.LeftForeArmStatus == ArmStatus.ArmMiddle) {
                return false;
            }

            double leftForearmAngle = GetAngle(joints[JointType.ShoulderLeft].AsVector3D(), joints[JointType.WristLeft].AsVector3D(), joints[JointType.ElbowLeft].AsVector3D());

            return leftForearmAngle > Angles.FORE_ARM_UP && leftForearmAngle < Angles.FORE_ARM_DOWN;
        }

        public override void Execute() {
            if (RoboManagerInstance.LeftForeArmStatus == ArmStatus.ArmUp) {
                RoboManagerInstance.LeftArmDown();
            }
            else {
                RoboManagerInstance.LeftArmUp();
            }
        }
    }
}