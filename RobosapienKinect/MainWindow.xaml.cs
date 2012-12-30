using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Timers;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Speech.Synthesis;
using UsbUirt;
using System.Windows.Documents;

using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

using Com.Enterprisecoding.RobosapienKinect.GestureCommands;
using Com.Enterprisecoding.RobosapienKinect.VoiceCommands;
using System.IO;

namespace Com.Enterprisecoding.RobosapienKinect {
    public partial class MainWindow : Window
    {
        #region Story Boards
        private readonly Storyboard microphoneInitializing;
        private readonly Storyboard speakNotRecognised;
        private readonly Storyboard speaking;

        private readonly Storyboard trackingStart;
        private readonly Storyboard trackingStop;
        
        private readonly Storyboard roboAppear;
        private readonly Storyboard roboDisappear;
        
        private readonly Storyboard leftForeArm;
        private readonly Storyboard leftArm;
        private readonly Storyboard rightForeArm;
        private readonly Storyboard rightArm;
        private readonly Storyboard upperBody;

        #endregion

        private SpeechSynthesizer speechSynthesizer;

        private KinectSensor kinectSensor;
        private readonly Controller usbuirtController = new Controller();
        private bool executeCommand = false;
        private readonly Timer commandTimeoutTimer = new Timer(20000); //20 saniye
        private SpeechRecognitionEngine speechRecognizer;
        private Stream kinectStream;
        private DispatcherTimer readyTimer;

        #region Commands
        private GestureCommandBase[] gestureCommands = new GestureCommandBase[]{
            new LeanBackwardCommand(),
            new LeanForwardCommand(),
            new LeftArmCenterCommand(),
            new LeftArmDownCommand(),
            new LeftArmInCommand(),
            new LeftArmMiddleCommand(),
            new LeftArmOutCommand(),
            new LeftArmUpCommand(),
            new LeftHandPickUpCommand(),
            new RightArmCenterCommand(),
            new RightArmDownCommand(),
            new RightArmInCommand(),
            new RightArmMiddleCommand(),
            new RightArmOutCommand(),
            new RightArmUpCommand(),
            new RightHandPickUpCommand(),
            new TiltBodyLeftCommand(),
            new TiltBodyRightCommand()
        };

        private Dictionary<string, VoiceCommandBase> voiceCommands = new Dictionary<string, VoiceCommandBase> {
            {WakeUpCommand.COMMAND_NAME.ToUpperInvariant(), new WakeUpCommand() },
            {SleepCommand.COMMAND_NAME.ToUpperInvariant(), new SleepCommand() },
            {PowerDownCommand.COMMAND_NAME.ToUpperInvariant(), new PowerDownCommand() },
            {ResetCommand.COMMAND_NAME.ToUpperInvariant(), new ResetCommand() },
            {StopCommand.COMMAND_NAME.ToUpperInvariant(), new StopCommand() },

            {FollowMeCommand.COMMAND_NAME.ToUpperInvariant(), new FollowMeCommand() },
            {DoNotFollowMeCommand.COMMAND_NAME.ToUpperInvariant(), new DoNotFollowMeCommand() },

            {DanceCommand.COMMAND_NAME.ToUpperInvariant(), new DanceCommand() },

            {StepBackCommand.COMMAND_NAME.ToUpperInvariant(), new StepBackCommand() },
            {StepForwardCommand.COMMAND_NAME.ToUpperInvariant(), new StepForwardCommand() },
            {StepLeftCommand.COMMAND_NAME.ToUpperInvariant(), new StepLeftCommand() },
            {StepRightCommand.COMMAND_NAME.ToUpperInvariant(), new StepRightCommand() },

            {WalkBackCommand.COMMAND_NAME.ToUpperInvariant(), new WalkBackCommand() },
            {WalkForwardCommand.COMMAND_NAME.ToUpperInvariant(), new WalkForwardCommand() },
            {WalkLeftCommand.COMMAND_NAME.ToUpperInvariant(), new WalkLeftCommand() },
            {WalkRightCommand.COMMAND_NAME.ToUpperInvariant(), new WalkRightCommand() }
        }; 
        #endregion

        public MainWindow() {
            InitializeComponent();

            commandTimeoutTimer.Elapsed += commandTimeoutTimer_Elapsed;

            var roboManagerInstance = RoboManager.Instance;
            roboManagerInstance.FollowUpChanged += roboManagerInstance_FollowUpChanged;

            #region Status change listeners
            roboManagerInstance.LeftArmStatusChanged += RoboManagerInstance_LeftArmStatusChanged;
            roboManagerInstance.LeftForeArmStatusChanged += RoboManagerInstance_LeftForeArmStatusChanged;
            roboManagerInstance.RightArmStatusChanged += RoboManagerInstance_RightArmStatusChanged;
            roboManagerInstance.RightForeArmStatusChanged += RoboManagerInstance_RightForeArmStatusChanged;
            #endregion

            microphoneInitializing = (Storyboard)TryFindResource("microphoneInitializing");
            speakNotRecognised = (Storyboard)TryFindResource("speakNotRecognised");
            speaking = (Storyboard)TryFindResource("speaking");

            trackingStart = (Storyboard)TryFindResource("trackingStart");
            trackingStop = (Storyboard)TryFindResource("trackingStop");

            roboAppear = (Storyboard)TryFindResource("roboAppear");
            roboDisappear = (Storyboard)TryFindResource("roboDisappear");

            leftForeArm = (Storyboard)TryFindResource("leftForeArm");
            leftArm = (Storyboard)TryFindResource("leftArm");
            rightForeArm = (Storyboard)TryFindResource("rightForeArm");
            rightArm = (Storyboard)TryFindResource("rightArm");
            upperBody = (Storyboard)TryFindResource("upperBody");
        }

        private void roboManagerInstance_FollowUpChanged(object sender, EventArgs e)
        {
            if (RoboManager.Instance.FollowUp) {
                appendLogEntry("İskelet takibi [AÇIK]");
                trackingStart.Begin();
            }
            else {
                appendLogEntry("İskelet takibi [KAPALI]");
                trackingStop.Begin();
            }


            usbuirtController.TransmitAsync(KumandaKodlari.Whistle, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }

        #region Arm status change listeners
        private void RoboManagerInstance_RightForeArmStatusChanged(object sender, ArmStatusEventArgs e)
        {
            if (!RoboManager.Instance.FollowUp) { return; }

            rightForeArm.Stop();
            rightForeArm.Begin();

            switch (e.NewStatus)
            {
                case ArmStatus.ArmDown:
                    appendLogEntry("Sağ Ön kol [Açıkta]");
                    usbuirtController.TransmitAsync(KumandaKodlari.RightArmOut, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
                case ArmStatus.ArmMiddle:
                    appendLogEntry("Sağ Ön kol [Ortada]");
                    usbuirtController.TransmitAsync(e.OldStatus == ArmStatus.ArmUp ? KumandaKodlari.RightArmOut : KumandaKodlari.RightArmIn, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
                case ArmStatus.ArmUp:
                    appendLogEntry("Sağ Ön kol [Kapalı]");
                    usbuirtController.TransmitAsync(KumandaKodlari.RightArmIn, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);

                    break;
            }
        }

        private void RoboManagerInstance_RightArmStatusChanged(object sender, ArmStatusEventArgs e)
        {
            if (!RoboManager.Instance.FollowUp) { return; }

            rightArm.Stop();
            rightArm.Begin();

            switch (e.NewStatus)
            {
                case ArmStatus.ArmDown:
                    appendLogEntry("Sağ kol [Aşağıda]");
                    usbuirtController.TransmitAsync(KumandaKodlari.RightArmDown, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
                case ArmStatus.ArmMiddle:
                    appendLogEntry("Sağ kol [Ortada]");
                    usbuirtController.TransmitAsync(e.OldStatus == ArmStatus.ArmDown ? KumandaKodlari.RightArmUp : KumandaKodlari.RightArmDown, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
                case ArmStatus.ArmUp:
                    appendLogEntry("Sağ kol [Yukarıda]");
                    usbuirtController.TransmitAsync(KumandaKodlari.RightArmUp, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
            }
        }

        private void RoboManagerInstance_LeftForeArmStatusChanged(object sender, ArmStatusEventArgs e)
        {
            if (!RoboManager.Instance.FollowUp) { return; }

            leftForeArm.Stop();
            leftForeArm.Begin();

            switch (e.NewStatus)
            {
                case ArmStatus.ArmDown:
                    appendLogEntry("Sol Ön kol [Açıkta]");
                    usbuirtController.TransmitAsync(KumandaKodlari.LeftArmOut, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
                case ArmStatus.ArmMiddle:
                    appendLogEntry("Sol Ön kol [Ortada]");
                    usbuirtController.TransmitAsync(e.OldStatus == ArmStatus.ArmUp ? KumandaKodlari.LeftArmOut : KumandaKodlari.LeftArmIn, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
                case ArmStatus.ArmUp:
                    appendLogEntry("Sol Ön kol [Kapalı]");
                    usbuirtController.TransmitAsync(KumandaKodlari.LeftArmIn, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);

                    break;
            }
        }

        private void RoboManagerInstance_LeftArmStatusChanged(object sender, ArmStatusEventArgs e)
        {
            if (!RoboManager.Instance.FollowUp) { return; }

            leftArm.Stop();
            leftArm.Begin();

            switch (e.NewStatus)
            {
                case ArmStatus.ArmDown:
                    appendLogEntry("Sol kol [Aşağıda]");
                    usbuirtController.TransmitAsync(KumandaKodlari.LeftArmDown, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
                case ArmStatus.ArmMiddle:
                    appendLogEntry("Sol kol [Ortada]");
                    usbuirtController.TransmitAsync(e.OldStatus == ArmStatus.ArmDown ? KumandaKodlari.LeftArmUp : KumandaKodlari.LeftArmDown, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
                case ArmStatus.ArmUp:
                    appendLogEntry("Sol kol [Yukarıda]");
                    usbuirtController.TransmitAsync(KumandaKodlari.LeftArmUp, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
                    break;
            }
        }
        #endregion

        private void commandTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => roboDisappear.Begin()), DispatcherPriority.Normal);
            executeCommand = false;
            commandTimeoutTimer.Stop();
        } 

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            speechSynthesizer = new SpeechSynthesizer();

            kinectSensor = (from sensorToCheck in KinectSensor.KinectSensors where sensorToCheck.Status == KinectStatus.Connected select sensorToCheck).FirstOrDefault();

            microphoneInitializing.Begin();
            if (kinectSensor!=null) {
                InitializeKinectServices();
            }

            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
        }
        
        private void appendLogEntry(string logMessage) {
            var p = new Paragraph();

            var logParts = logMessage.Split('[');
            p.Inlines.Add(new Run(logParts[0] + "["));

            logParts = logParts[1].Split(']');
            p.Inlines.Add(new Bold(new Run(logParts[0])) { Foreground = Brushes.Red });

            p.Inlines.Add(new Run("]"));

            logs.Document.Blocks.Add(p);

            logs.ScrollToEnd();
        }

        private void appendColoredLogEntry(string logMessage, Brush color)  {
            var range = new TextRange(logs.Document.ContentEnd, logs.Document.ContentEnd) {Text = logMessage};
            range.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        private void InitializeKinectServices() {
            appendLogEntry("Kinect [Bağlı]");

            kinectSensor.SkeletonFrameReady += kinectSensor_SkeletonFrameReady;

            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            kinectSensor.SkeletonStream.Enable();

            colorViewer.Kinect = kinectSensor;
            skeletonViewer.Kinect = kinectSensor;

            speechRecognizer = CreateSpeechRecognizer();

            kinectSensor.Start();

            if (speechRecognizer == null) { return; }

            readyTimer = new DispatcherTimer();
            readyTimer.Tick += ReadyTimerTick;
            readyTimer.Interval = new TimeSpan(0, 0, 4);
            readyTimer.Start();
        }

        private void UninitializeKinectServices() {
            appendLogEntry("Kinect [Bağlı değil]");

            kinectSensor.SkeletonFrameReady -= kinectSensor_SkeletonFrameReady;
            
            kinectSensor.Stop();

            speechRecognizer.RecognizeAsyncCancel();
            speechRecognizer.RecognizeAsyncStop();

            kinectStream = null;
            colorViewer.Kinect = null;
            skeletonViewer.Kinect = null;

            if (kinectSensor.SkeletonStream != null) {
                kinectSensor.SkeletonStream.Disable();
            }

            if (readyTimer == null) { return; }

            readyTimer.Stop();
            readyTimer = null;
        }

        private void ReadyTimerTick(object sender, EventArgs e)
        {
            Start();
            readyTimer.Stop();
            readyTimer = null;
        }

        private void Start() {
            var audioSource = kinectSensor.AudioSource;
            audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
            var kinectStream = audioSource.Start();
            speechRecognizer.SetInputToAudioStream(kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);

            appendLogEntry("Kinect [Ses tanıma açık]");
            microphoneInitializing.Stop();
        }

        private SpeechRecognitionEngine CreateSpeechRecognizer()
        {
            RecognizerInfo ri = GetKinectRecognizer();
            if (ri == null) {
                MessageBox.Show(
                    @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed.",
                    "Failed to load Speech SDK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
                return null;
            }

            SpeechRecognitionEngine sre;
            try {
                sre = new SpeechRecognitionEngine(ri.Id);
            }
            catch
            {
                MessageBox.Show(
                    @"There was a problem initializing Speech Recognition.
Ensure you have the Microsoft Speech SDK installed and configured.",
                    "Failed to load Speech SDK",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Close();
                return null;
            }

            var commands = new Choices();

            commands.Add("robo");
            foreach (var command in voiceCommands.Values) {
                commands.Add(command.Command);
            }

            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(commands);

            // Create the actual Grammar instance, and then load it into the speech recognizer.
            var g = new Grammar(gb);

            sre.LoadGrammar(g);
            sre.SpeechRecognized += sre_SpeechRecognized;
            sre.SpeechRecognitionRejected += sre_SpeechRecognitionRejected;
            sre.SpeechDetected += sre_SpeechDetected;

            return sre;
        }

        private void sre_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            speaking.Stop();
            speaking.Begin();

            appendLogEntry(" [Konuşma Algılandı]");
        }

        private void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e) {
            speakNotRecognised.Stop();
            speakNotRecognised.Begin();

            appendLogEntry(" [Komut Reddedildi]");
        }

        private void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e) {
            if (e.Result.Confidence < 0.8) { return; }

            var command = e.Result.Text.ToUpperInvariant();

            if (command == "ROBO") {
                roboAppear.Begin();
                appendLogEntry("Komut [ROBO]");
                executeCommand = true;
                commandTimeoutTimer.Start();
                return;
            }

            if (!executeCommand) { return; }

            if (commandTimeoutTimer.Enabled) {
                commandTimeoutTimer.Stop();
                roboDisappear.Begin();
            }

            appendLogEntry("Komut [" + command + "]");

            executeCommand = false;
            voiceCommands[command].Execute();
        }

        private void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
            using (var skeletonFrame = e.OpenSkeletonFrame()) {
                if (skeletonFrame == null) { return; }

                var skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

                skeletonFrame.CopySkeletonDataTo(skeletonData);

                foreach (var skelaton in skeletonData.Where(skelaton => skelaton.TrackingState == SkeletonTrackingState.Tracked)) {
                    foreach (var gestureCommand in gestureCommands.Where(gestureCommand => gestureCommand.ShouldHandle(skelaton.Joints))) {
                        gestureCommand.Execute();
                    }

                    return;
                }
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e) {
            switch (e.Status) {
                case KinectStatus.Connected:
                    kinectSensor = (from sensorToCheck in KinectSensor.KinectSensors where sensorToCheck.Status == KinectStatus.Connected select sensorToCheck).FirstOrDefault();
                    InitializeKinectServices();
                    break;
                case KinectStatus.Disconnected:
                    UninitializeKinectServices();
                    kinectSensor = null;
                    break;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e) {
            if (kinectSensor!=null) {
                UninitializeKinectServices(); 
            }
        }

        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }
    }
}
