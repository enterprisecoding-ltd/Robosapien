namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class StepForwardCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "step forward";

        public StepForwardCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            TransmitCommand(KumandaKodlari.MoveStepForward);
        }
    }
}
