//------------------------------------------------------------------------------
// <copyright file="KinectSkeletonViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.Viewers {
    public enum ImageType {
        Color,
        Depth,
    }

    internal enum TrackingMode {
        DefaultSystemTracking,
        Closest1Player,
        Closest2Player,
        Sticky1Player,
        Sticky2Player,
        MostActive1Player,
        MostActive2Player
    }

    /// <summary>
    ///     Interaction logic for KinectSkeletonViewer.xaml
    /// </summary>
    public partial class KinectSkeletonViewer : ImageViewer, INotifyPropertyChanged {
        private const float ActivityFalloff = 0.98f;
        private readonly List<int> activeList = new List<int>();
        private readonly List<Dictionary<JointType, JointMapping>> jointMappings = new List<Dictionary<JointType, JointMapping>>();
        private readonly List<ActivityWatcher> recentActivity = new List<ActivityWatcher>();
        private List<KinectSkeleton> skeletonCanvases;
        private Skeleton[] skeletonData;

        public KinectSkeletonViewer() {
            InitializeComponent();
            ShowJoints = true;
            ShowBones = true;
            ShowCenter = true;
        }

        public bool ShowBones { get; set; }

        public bool ShowJoints { get; set; }

        public bool ShowCenter { get; set; }

        public ImageType ImageType { get; set; }

        internal TrackingMode TrackingMode { get; set; }

        public void HideAllSkeletons() {
            if (skeletonCanvases != null) {
                foreach (KinectSkeleton skeletonCanvas in skeletonCanvases) {
                    skeletonCanvas.Reset();
                }
            }
        }

        protected override void OnKinectChanged(KinectSensor oldKinectSensor, KinectSensor newKinectSensor) {
            if (oldKinectSensor != null) {
                oldKinectSensor.AllFramesReady -= KinectAllFramesReady;
                HideAllSkeletons();
            }

            if (newKinectSensor != null && newKinectSensor.Status == KinectStatus.Connected) {
                newKinectSensor.AllFramesReady += KinectAllFramesReady;
            }
        }

        private void KinectAllFramesReady(object sender, AllFramesReadyEventArgs e) {
            // Have we already been "shut down" by the user of this viewer, 
            // or has the SkeletonStream been disabled since this event was posted?
            if ((Kinect == null) || !((KinectSensor) sender).SkeletonStream.IsEnabled) {
                return;
            }

            bool haveSkeletonData = false;

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) {
                if (skeletonFrame != null) {
                    if (skeletonCanvases == null) {
                        CreateListOfSkeletonCanvases();
                    }

                    if ((skeletonData == null) || (skeletonData.Length != skeletonFrame.SkeletonArrayLength)) {
                        skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    haveSkeletonData = true;
                }
            }

            if (haveSkeletonData) {
                using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame()) {
                    if (depthImageFrame != null) {
                        int trackedSkeletons = 0;

                        foreach (Skeleton skeleton in skeletonData) {
                            Dictionary<JointType, JointMapping> jointMapping = jointMappings[trackedSkeletons];
                            jointMapping.Clear();

                            KinectSkeleton skeletonCanvas = skeletonCanvases[trackedSkeletons++];
                            skeletonCanvas.ShowBones = ShowBones;
                            skeletonCanvas.ShowJoints = ShowJoints;
                            skeletonCanvas.ShowCenter = ShowCenter;

                            // Transform the data into the correct space
                            // For each joint, we determine the exact X/Y coordinates for the target view
                            foreach (Joint joint in skeleton.Joints) {
                                Point mappedPoint = GetPosition2DLocation(depthImageFrame, joint.Position);
                                jointMapping[joint.JointType] = new JointMapping {
                                    Joint = joint,
                                    MappedPoint = mappedPoint
                                };
                            }

                            // Look up the center point
                            Point centerPoint = GetPosition2DLocation(depthImageFrame, skeleton.Position);

                            // Scale the skeleton thickness
                            // 1.0 is the desired size at 640 width
                            double scale = RenderSize.Width/640;

                            skeletonCanvas.RefreshSkeleton(skeleton, jointMapping, centerPoint, scale);
                        }

                        if (ImageType == ImageType.Depth) {
                            ChooseTrackedSkeletons(skeletonData);
                        }
                    }
                }
            }
        }

        private Point GetPosition2DLocation(DepthImageFrame depthFrame, SkeletonPoint skeletonPoint) {
            DepthImagePoint depthPoint = Kinect.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, Kinect.DepthStream.Format);

            switch (ImageType) {
                case ImageType.Color:
                    ColorImagePoint colorPoint = Kinect.CoordinateMapper.MapDepthPointToColorPoint(Kinect.DepthStream.Format, depthPoint, Kinect.ColorStream.Format);

                    // map back to skeleton.Width & skeleton.Height
                    return new Point(
                        (int) (RenderSize.Width*colorPoint.X/Kinect.ColorStream.FrameWidth),
                        (int) (RenderSize.Height*colorPoint.Y/Kinect.ColorStream.FrameHeight));
                case ImageType.Depth:
                    return new Point(
                        (int) (RenderSize.Width*depthPoint.X/depthFrame.Width),
                        (int) (RenderSize.Height*depthPoint.Y/depthFrame.Height));
                default:
                    throw new ArgumentOutOfRangeException("ImageType was a not expected value: " + ImageType.ToString());
            }
        }

        private void CreateListOfSkeletonCanvases() {
            skeletonCanvases = new List<KinectSkeleton> {
                skeletonCanvas1,
                skeletonCanvas2,
                skeletonCanvas3,
                skeletonCanvas4,
                skeletonCanvas5,
                skeletonCanvas6
            };

            skeletonCanvases.ForEach(s => jointMappings.Add(new Dictionary<JointType, JointMapping>()));
        }

        // NOTE: The ChooseTrackedSkeletons part of the KinectSkeletonViewer would be useful
        // separate from the SkeletonViewer.
        private void ChooseTrackedSkeletons(IEnumerable<Skeleton> skeletonDataValue) {
            switch (TrackingMode) {
                case TrackingMode.Closest1Player:
                    ChooseClosestSkeletons(skeletonDataValue, 1);
                    break;
                case TrackingMode.Closest2Player:
                    ChooseClosestSkeletons(skeletonDataValue, 2);
                    break;
                case TrackingMode.Sticky1Player:
                    ChooseOldestSkeletons(skeletonDataValue, 1);
                    break;
                case TrackingMode.Sticky2Player:
                    ChooseOldestSkeletons(skeletonDataValue, 2);
                    break;
                case TrackingMode.MostActive1Player:
                    ChooseMostActiveSkeletons(skeletonDataValue, 1);
                    break;
                case TrackingMode.MostActive2Player:
                    ChooseMostActiveSkeletons(skeletonDataValue, 2);
                    break;
            }
        }

        private void ChooseClosestSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count) {
            var depthSorted = new SortedList<float, int>();

            foreach (Skeleton s in skeletonDataValue) {
                if (s.TrackingState != SkeletonTrackingState.NotTracked) {
                    float valueZ = s.Position.Z;
                    while (depthSorted.ContainsKey(valueZ)) {
                        valueZ += 0.0001f;
                    }

                    depthSorted.Add(valueZ, s.TrackingId);
                }
            }

            ChooseSkeletonsFromList(depthSorted.Values, count);
        }

        private void ChooseOldestSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count) {
            var newList = new List<int>();

            foreach (Skeleton s in skeletonDataValue) {
                if (s.TrackingState != SkeletonTrackingState.NotTracked) {
                    newList.Add(s.TrackingId);
                }
            }

            // Remove all elements from the active list that are not currently present
            activeList.RemoveAll(k => !newList.Contains(k));

            // Add all elements that aren't already in the activeList
            activeList.AddRange(newList.FindAll(k => !activeList.Contains(k)));

            ChooseSkeletonsFromList(activeList, count);
        }

        private void ChooseMostActiveSkeletons(IEnumerable<Skeleton> skeletonDataValue, int count) {
            foreach (ActivityWatcher watcher in recentActivity) {
                watcher.NewPass();
            }

            foreach (Skeleton s in skeletonDataValue) {
                if (s.TrackingState != SkeletonTrackingState.NotTracked) {
                    ActivityWatcher watcher = recentActivity.Find(w => w.TrackingId == s.TrackingId);
                    if (watcher != null) {
                        watcher.Update(s);
                    }
                    else {
                        recentActivity.Add(new ActivityWatcher(s));
                    }
                }
            }

            // Remove any skeletons that are gone
            recentActivity.RemoveAll(aw => !aw.Updated);

            recentActivity.Sort();
            ChooseSkeletonsFromList(recentActivity.ConvertAll(f => f.TrackingId), count);
        }

        private void ChooseSkeletonsFromList(IList<int> list, int max) {
            if (Kinect.SkeletonStream.IsEnabled) {
                int argCount = Math.Min(list.Count, max);

                if (argCount == 0) {
                    Kinect.SkeletonStream.ChooseSkeletons();
                }

                if (argCount == 1) {
                    Kinect.SkeletonStream.ChooseSkeletons(list[0]);
                }

                if (argCount >= 2) {
                    Kinect.SkeletonStream.ChooseSkeletons(list[0], list[1]);
                }
            }
        }

        private class ActivityWatcher : IComparable<ActivityWatcher> {
            private float activityLevel;
            private SkeletonPoint previousDelta;
            private SkeletonPoint previousPosition;

            internal ActivityWatcher(Skeleton s) {
                activityLevel = 0.0f;
                TrackingId = s.TrackingId;
                Updated = true;
                previousPosition = s.Position;
                previousDelta = new SkeletonPoint();
            }

            internal int TrackingId { get; private set; }

            internal bool Updated { get; private set; }

            public int CompareTo(ActivityWatcher other) {
                // Use the existing CompareTo on float, but reverse the arguments,
                // since we wish to have larger activityLevels sort ahead of smaller values.
                return other.activityLevel.CompareTo(activityLevel);
            }

            internal void NewPass() {
                Updated = false;
            }

            internal void Update(Skeleton s) {
                SkeletonPoint newPosition = s.Position;
                var newDelta = new SkeletonPoint {
                    X = newPosition.X - previousPosition.X,
                    Y = newPosition.Y - previousPosition.Y,
                    Z = newPosition.Z - previousPosition.Z
                };

                var deltaV = new SkeletonPoint {
                    X = newDelta.X - previousDelta.X,
                    Y = newDelta.Y - previousDelta.Y,
                    Z = newDelta.Z - previousDelta.Z
                };

                previousPosition = newPosition;
                previousDelta = newDelta;

                float deltaVLengthSquared = (deltaV.X*deltaV.X) + (deltaV.Y*deltaV.Y) + (deltaV.Z*deltaV.Z);
                var deltaVLength = (float) Math.Sqrt(deltaVLengthSquared);

                activityLevel = activityLevel*ActivityFalloff;
                activityLevel += deltaVLength;

                Updated = true;
            }
        }
    }
}