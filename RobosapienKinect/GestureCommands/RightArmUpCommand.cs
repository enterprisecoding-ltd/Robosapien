using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class RightArmUpCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.RightForeArmStatus == ArmStatus.ArmUp) {
                return false;
            }

            double rightForearmAngle = GetAngle(joints[JointType.ShoulderRight].AsVector3D(), joints[JointType.WristRight].AsVector3D(), joints[JointType.ElbowRight].AsVector3D());

            return rightForearmAngle <= Angles.FORE_ARM_UP;
        }

        public override void Execute() {
            RoboManagerInstance.RightArmUp();
        }
    }
}