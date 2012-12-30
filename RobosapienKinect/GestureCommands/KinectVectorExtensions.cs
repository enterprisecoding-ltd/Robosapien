using System.Windows.Media.Media3D;
using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.GestureCommands {
    internal static class KinectVectorExtensions  {
        public static Vector3D AsVector3D(this Joint joint) {
            var jointPosition = joint.Position;

            return new Vector3D(jointPosition.X, jointPosition.Y, jointPosition.Z);
        }
    }
}