using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class RightArmDownCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.RightForeArmStatus == ArmStatus.ArmDown) { return false; }

            var rightForearmAngle = GetAngle(joints[JointType.ShoulderRight].AsVector3D(), joints[JointType.WristRight].AsVector3D(), joints[JointType.ElbowRight].AsVector3D());

            return rightForearmAngle >= Angles.FORE_ARM_DOWN;
        }

        public override void Execute() {
            RoboManagerInstance.RightArmDown();
        }
    }
}
