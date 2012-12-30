using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class RightArmMiddleCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.RightForeArmStatus == ArmStatus.ArmMiddle) { return false; }

            var rightForearmAngle = GetAngle(joints[JointType.ShoulderRight].AsVector3D(), joints[JointType.WristRight].AsVector3D(), joints[JointType.ElbowRight].AsVector3D());

            return rightForearmAngle > Angles.FORE_ARM_UP && rightForearmAngle < Angles.FORE_ARM_DOWN;
        }

        public override void Execute() {
            if (RoboManagerInstance.RightForeArmStatus == ArmStatus.ArmUp) {
                RoboManagerInstance.RightArmDown();
            }
            else{
                RoboManagerInstance.RightArmUp();
            }
        }
    }
}