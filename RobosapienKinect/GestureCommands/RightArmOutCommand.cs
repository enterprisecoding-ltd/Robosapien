using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class RightArmOutCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            if (RoboManagerInstance.RightArmStatus == ArmStatus.ArmUp) {
                return false;
            }

            Vector3D shoulderCenter = joints[JointType.ShoulderCenter].AsVector3D();

            double rightArmAngle = GetAngle(joints[JointType.ElbowRight].AsVector3D() - shoulderCenter,
                                            joints[JointType.Spine].AsVector3D() - shoulderCenter,
                                            shoulderCenter);

            return rightArmAngle >= Angles.ARM_OUT;
        }

        public override void Execute() {
            RoboManagerInstance.RightArmOut();
        }
    }
}