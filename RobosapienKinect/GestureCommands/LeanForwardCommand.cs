using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class LeanForwardCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {
            return false;
        }

        public override void Execute() {
            RoboManagerInstance.LeanForward();
        }
    }
}