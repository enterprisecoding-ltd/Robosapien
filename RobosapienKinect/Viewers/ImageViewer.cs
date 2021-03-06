//------------------------------------------------------------------------------
// <copyright file="ImageViewer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Kinect;

namespace Com.Enterprisecoding.RobosapienKinect.Viewers {
    public abstract class ImageViewer : UserControl, INotifyPropertyChanged {
        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register("Stretch", typeof (Stretch), typeof (ImageViewer), new UIPropertyMetadata(Stretch.Uniform));

        public static readonly DependencyProperty KinectProperty =
            DependencyProperty.Register("Kinect", typeof (KinectSensor), typeof (ImageViewer), new UIPropertyMetadata(null, KinectChanged));

        private bool collectFrameRate;

        private bool flipHorizontally;
        private int frameRate = -1;
        private ScaleTransform horizontalScaleTransform;
        private DateTime lastTime = DateTime.MaxValue;

        public KinectSensor Kinect {
            get { return (KinectSensor) GetValue(KinectProperty); }

            set { SetValue(KinectProperty, value); }
        }

        public bool FlipHorizontally {
            get { return flipHorizontally; }

            set {
                if (flipHorizontally != value) {
                    flipHorizontally = value;
                    NotifyPropertyChanged("FlipHorizontally");
                    horizontalScaleTransform = new ScaleTransform {ScaleX = flipHorizontally ? -1 : 1};
                    NotifyPropertyChanged("HorizontalScaleTransform");
                }
            }
        }

        public ScaleTransform HorizontalScaleTransform {
            get { return horizontalScaleTransform; }
        }

        public Stretch Stretch {
            get { return (Stretch) GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public bool CollectFrameRate {
            get { return collectFrameRate; }

            set {
                if (value != collectFrameRate) {
                    collectFrameRate = value;
                    NotifyPropertyChanged("CollectFrameRate");
                }
            }
        }

        public int FrameRate {
            get { return frameRate; }

            private set {
                if (frameRate != value) {
                    frameRate = value;
                    NotifyPropertyChanged("FrameRate");
                }
            }
        }

        protected int TotalFrames { get; set; }

        protected int LastFrames { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected abstract void OnKinectChanged(KinectSensor oldKinectSensor, KinectSensor newKinectSensor);

        protected void ResetFrameRateCounters() {
            if (CollectFrameRate) {
                lastTime = DateTime.MaxValue;
                TotalFrames = 0;
                LastFrames = 0;
            }
        }

        protected void UpdateFrameRate() {
            if (CollectFrameRate) {
                ++TotalFrames;

                DateTime cur = DateTime.Now;
                TimeSpan span = cur.Subtract(lastTime);
                if (lastTime == DateTime.MaxValue || span >= TimeSpan.FromSeconds(1)) {
                    // A straight cast will truncate the value, leading to chronic under-reporting of framerate.
                    // rounding yields a more balanced result
                    FrameRate = (int) Math.Round((TotalFrames - LastFrames)/span.TotalSeconds);
                    LastFrames = TotalFrames;
                    lastTime = cur;
                }
            }
        }

        protected void NotifyPropertyChanged(string info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private static void KinectChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            var imageViewer = (ImageViewer) d;
            imageViewer.OnKinectChanged((KinectSensor) args.OldValue, (KinectSensor) args.NewValue);
        }
    }
}