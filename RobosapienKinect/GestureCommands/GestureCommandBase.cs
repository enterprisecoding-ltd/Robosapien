using Microsoft.Kinect;
using System.Windows.Media.Media3D;
using UsbUirt;
using System;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal abstract class GestureCommandBase {
        private const double RADIAN2DEGREE= 180.0 / Math.PI;
        private static readonly Vector3D normalVector = new Vector3D(0, 0, 1);

        protected readonly RoboManager RoboManagerInstance = RoboManager.Instance;

        public abstract bool ShouldHandle(JointCollection joints);

        public abstract void Execute();

        protected double GetAngle(Vector3D firstVector, Vector3D secondVector, Vector3D intersectionVector) {
            
            var b1 = intersectionVector - firstVector;
            var b2 = intersectionVector - secondVector;

            b1.Normalize();
            b2.Normalize();

            var dotProduct = Vector3D.DotProduct(b1, b2);

            return (double)Math.Acos(dotProduct) * RADIAN2DEGREE; 
        }
    }
}