using System;
using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal abstract class GestureCommandBase {
        private const double RADIAN2DEGREE = 180.0/Math.PI;
        private static readonly Vector3D normalVector = new Vector3D(0, 0, 1);

        protected readonly RoboManager RoboManagerInstance = RoboManager.Instance;

        public abstract bool ShouldHandle(JointCollection joints);

        public abstract void Execute();

        protected double GetAngle(Vector3D firstVector, Vector3D secondVector, Vector3D intersectionVector) {
            Vector3D b1 = intersectionVector - firstVector;
            Vector3D b2 = intersectionVector - secondVector;

            b1.Normalize();
            b2.Normalize();

            double dotProduct = Vector3D.DotProduct(b1, b2);

            return Math.Acos(dotProduct)*RADIAN2DEGREE;
        }
    }
}