using Microsoft.Kinect;
using System;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal sealed class LeanBackwardCommand : GestureCommandBase {
        public override bool ShouldHandle(JointCollection joints) {

            return false;
        }

        public override void Execute() {
            RoboManagerInstance.LeanBackward();
        }
    }
}