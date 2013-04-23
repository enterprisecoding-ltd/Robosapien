//------------------------------------------------------------------------------
// <copyright file="KinectSkeleton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.Viewers {
    /// <summary>
    ///     This control is used to render a player's skeleton.
    ///     If the ClipToBounds is set to "false", it will be allowed to overdraw
    ///     it's bounds.
    /// </summary>
    public class KinectSkeleton : Control {
        private const double JointThickness = 3;
        private const double BodyCenterThickness = 10;
        private const double TrackedBoneThickness = 6;
        private const double InferredBoneThickness = 1;
        private const double ClipBoundsThickness = 10;

        public static readonly DependencyProperty ShowClippedEdgesProperty =
            DependencyProperty.Register("ShowClippedEdges", typeof (bool), typeof (KinectSkeleton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowJointsProperty =
            DependencyProperty.Register("ShowJoints", typeof (bool), typeof (KinectSkeleton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowBonesProperty =
            DependencyProperty.Register("ShowBones", typeof (bool), typeof (KinectSkeleton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ShowCenterProperty =
            DependencyProperty.Register("ShowCenter", typeof (bool), typeof (KinectSkeleton), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

        private readonly Brush bottomClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 0), new Point(0, 1));

        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, InferredBoneThickness);
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        private readonly Brush leftClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(1, 0), new Point(0, 0));

        private readonly Brush rightClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 0), new Point(1, 0));

        private readonly Brush topClipBrush = new LinearGradientBrush(
            Color.FromArgb(0, 255, 0, 0), Color.FromArgb(255, 255, 0, 0), new Point(0, 1), new Point(0, 0));

        private readonly Pen trackedBonePen = new Pen(Brushes.Green, TrackedBoneThickness);
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        private Point centerPoint;
        private Skeleton currentSkeleton;
        private Dictionary<JointType, JointMapping> jointMappings;
        private double scale = 1.0;

        public bool ShowClippedEdges {
            get { return (bool) GetValue(ShowClippedEdgesProperty); }
            set { SetValue(ShowClippedEdgesProperty, value); }
        }

        public bool ShowJoints {
            get { return (bool) GetValue(ShowJointsProperty); }
            set { SetValue(ShowJointsProperty, value); }
        }

        public bool ShowBones {
            get { return (bool) GetValue(ShowBonesProperty); }
            set { SetValue(ShowBonesProperty, value); }
        }

        public bool ShowCenter {
            get { return (bool) GetValue(ShowCenterProperty); }
            set { SetValue(ShowCenterProperty, value); }
        }

        /// <summary>
        ///     This method should be called every skeleton frame.  It will force the properties to update and
        ///     trigger the control to render.
        /// </summary>
        /// <param name="skeleton">This is the current frame's skeleton data.</param>
        /// <param name="mappings">This is a list of pre-mapped joints.  See KinectSkeletonViewerer.xaml.cs</param>
        /// <param name="center">This is a pre-mapped point to the skeleton's center position.</param>
        /// <param name="scaleFactor">
        ///     1 will render the bones and joints at a good size for a 640x480 image.
        ///     The method would expect 0.5 to render a 320x240 scaled image.
        /// </param>
        public void RefreshSkeleton(Skeleton skeleton, Dictionary<JointType, JointMapping> mappings, Point center, double scaleFactor) {
            centerPoint = center;
            jointMappings = mappings;
            scale = scaleFactor;
            currentSkeleton = skeleton;

            InvalidateVisual();
        }

        public void Reset() {
            currentSkeleton = null;
            InvalidateVisual();
        }

        protected override Size MeasureOverride(Size constraint) {
            return new Size();
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            return arrangeBounds;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);

            // Don't render if we don't have a skeleton, or it isn't tracked
            if (currentSkeleton == null || currentSkeleton.TrackingState == SkeletonTrackingState.NotTracked) {
                return;
            }

            // Displays a gradient near the edge of the display where the skeleton is leaving the screen
            RenderClippedEdges(drawingContext);

            switch (currentSkeleton.TrackingState) {
                case SkeletonTrackingState.PositionOnly:
                    if (ShowCenter) {
                        drawingContext.DrawEllipse(
                            centerPointBrush,
                            null,
                            centerPoint,
                            BodyCenterThickness*scale,
                            BodyCenterThickness*scale);
                    }

                    break;
                case SkeletonTrackingState.Tracked:
                    DrawBonesAndJoints(drawingContext);
                    break;
            }
        }

        private void RenderClippedEdges(DrawingContext drawingContext) {
            if (!ShowClippedEdges ||
                currentSkeleton.ClippedEdges.Equals(FrameEdges.None)) {
                return;
            }

            double scaledThickness = ClipBoundsThickness*scale;
            if (currentSkeleton.ClippedEdges.HasFlag(FrameEdges.Bottom)) {
                drawingContext.DrawRectangle(
                    bottomClipBrush,
                    null,
                    new Rect(0, RenderSize.Height - scaledThickness, RenderSize.Width, scaledThickness));
            }

            if (currentSkeleton.ClippedEdges.HasFlag(FrameEdges.Top)) {
                drawingContext.DrawRectangle(
                    topClipBrush,
                    null,
                    new Rect(0, 0, RenderSize.Width, scaledThickness));
            }

            if (currentSkeleton.ClippedEdges.HasFlag(FrameEdges.Left)) {
                drawingContext.DrawRectangle(
                    leftClipBrush,
                    null,
                    new Rect(0, 0, scaledThickness, RenderSize.Height));
            }

            if (currentSkeleton.ClippedEdges.HasFlag(FrameEdges.Right)) {
                drawingContext.DrawRectangle(
                    rightClipBrush,
                    null,
                    new Rect(RenderSize.Width - scaledThickness, 0, scaledThickness, RenderSize.Height));
            }
        }

        private void DrawBonesAndJoints(DrawingContext drawingContext) {
            if (ShowBones) {
                // Render Torso
                DrawBone(drawingContext, JointType.Head, JointType.ShoulderCenter);
                DrawBone(drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
                DrawBone(drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
                DrawBone(drawingContext, JointType.ShoulderCenter, JointType.Spine);
                DrawBone(drawingContext, JointType.Spine, JointType.HipCenter);
                DrawBone(drawingContext, JointType.HipCenter, JointType.HipLeft);
                DrawBone(drawingContext, JointType.HipCenter, JointType.HipRight);

                // Left Arm
                DrawBone(drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
                DrawBone(drawingContext, JointType.ElbowLeft, JointType.WristLeft);
                DrawBone(drawingContext, JointType.WristLeft, JointType.HandLeft);

                // Right Arm
                DrawBone(drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
                DrawBone(drawingContext, JointType.ElbowRight, JointType.WristRight);
                DrawBone(drawingContext, JointType.WristRight, JointType.HandRight);

                // Left Leg
                DrawBone(drawingContext, JointType.HipLeft, JointType.KneeLeft);
                DrawBone(drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
                DrawBone(drawingContext, JointType.AnkleLeft, JointType.FootLeft);

                // Right Leg
                DrawBone(drawingContext, JointType.HipRight, JointType.KneeRight);
                DrawBone(drawingContext, JointType.KneeRight, JointType.AnkleRight);
                DrawBone(drawingContext, JointType.AnkleRight, JointType.FootRight);
            }

            if (ShowJoints) {
                // Render Joints
                foreach (JointMapping joint in jointMappings.Values) {
                    Brush drawBrush = null;
                    switch (joint.Joint.TrackingState) {
                        case JointTrackingState.Tracked:
                            drawBrush = trackedJointBrush;
                            break;
                        case JointTrackingState.Inferred:
                            drawBrush = inferredJointBrush;
                            break;
                    }

                    if (drawBrush != null) {
                        drawingContext.DrawEllipse(drawBrush, null, joint.MappedPoint, JointThickness*scale, JointThickness*scale);
                    }
                }
            }
        }

        private void DrawBone(DrawingContext drawingContext, JointType jointType1, JointType jointType2) {
            JointMapping joint1;
            JointMapping joint2;

            // If we can't find either of these joints, exit
            if (!jointMappings.TryGetValue(jointType1, out joint1) ||
                joint1.Joint.TrackingState == JointTrackingState.NotTracked ||
                !jointMappings.TryGetValue(jointType2, out joint2) ||
                joint2.Joint.TrackingState == JointTrackingState.NotTracked) {
                return;
            }

            // Don't draw if both points are inferred
            if (joint1.Joint.TrackingState == JointTrackingState.Inferred &&
                joint2.Joint.TrackingState == JointTrackingState.Inferred) {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = inferredBonePen;
            drawPen.Thickness = InferredBoneThickness*scale;
            if (joint1.Joint.TrackingState == JointTrackingState.Tracked && joint2.Joint.TrackingState == JointTrackingState.Tracked) {
                drawPen = trackedBonePen;
                drawPen.Thickness = TrackedBoneThickness*scale;
            }

            drawingContext.DrawLine(drawPen, joint1.MappedPoint, joint2.MappedPoint);
        }
    }
}