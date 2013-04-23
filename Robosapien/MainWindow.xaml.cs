using System;
using System.Windows;
using UsbUirt;

namespace Com.Enterprisecoding.Robosapien {
    public partial class MainWindow : Window {
        #region Data Members

        private readonly Controller usbuirtController;
        private bool sleep = true;

        #endregion

        public MainWindow() {
            InitializeComponent();

            usbuirtController = new Controller();
        }

        #region Member Functions

        #region Lean

        private void LeanRight_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeanRight, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void LeanLeft_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeanLeft, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void LeanBack_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeanBack, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void LeanForward_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeanForward, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        #endregion

        #region Left Arm

        private void LeftArmAllIn_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeftArmIn, KumandaKodlari.KodFormati, 2, KumandaKodlari.DoubleCommmandTimeSpan);
        }

        private void LeftArmIn_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeftArmIn, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void LeftArmAllUp_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeftArmUp, KumandaKodlari.KodFormati, 2, KumandaKodlari.DoubleCommmandTimeSpan);
        }

        private void LeftArmUp_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeftArmUp, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void LeftArmDown_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeftArmDown, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void LeftArmAllDown_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeftArmDown, KumandaKodlari.KodFormati, 2, KumandaKodlari.DoubleCommmandTimeSpan);
        }

        private void LeftArmOut_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeftArmOut, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void LeftArmAllOut_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.LeftArmOut, KumandaKodlari.KodFormati, 2, KumandaKodlari.DoubleCommmandTimeSpan);
        }

        #endregion

        #region Move

        #region Walk

        private void MoveWalkLeft_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.MoveWalkLeft, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void MoveWalkRight_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.MoveWalkRight, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void MoveWalkForward_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.MoveWalkForward, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void MoveWalkBack_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.MoveWalkBack, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        #endregion

        private void MoveStop_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.Stop, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        #region Step

        private void MoveStepForward_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.MoveStepForward, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void MoveStepBack_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.MoveStepBack, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void MoveStepRight_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.MoveStepRight, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void MoveStepLeft_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.MoveStepLeft, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        #endregion

        #endregion

        #region Right Arm

        private void RightArmAllOut_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.RightArmOut, KumandaKodlari.KodFormati, 2, KumandaKodlari.DoubleCommmandTimeSpan);
        }

        private void RightArmAllIn_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.RightArmIn, KumandaKodlari.KodFormati, 2, KumandaKodlari.DoubleCommmandTimeSpan);
        }

        private void RightArmIn_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.RightArmIn, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void RightArmOut_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.RightArmOut, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void RightArmAllUp_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.RightArmUp, KumandaKodlari.KodFormati, 2, KumandaKodlari.DoubleCommmandTimeSpan);
        }

        private void RightArmUp_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.RightArmUp, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void RightArmDown_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.RightArmDown, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        private void RightArmAllDown_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(KumandaKodlari.RightArmDown, KumandaKodlari.KodFormati, 2, KumandaKodlari.DoubleCommmandTimeSpan);
        }

        private void Power_Click(object sender, RoutedEventArgs e) {
            usbuirtController.Transmit(sleep ? KumandaKodlari.WakeUp : KumandaKodlari.Sleep, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
            sleep = !sleep;
        }

        #endregion

        #endregion
    }
}