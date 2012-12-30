namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class StepLeftCommand: VoiceCommandBase {
        public const string COMMAND_NAME = "step left";

        public StepLeftCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            TransmitCommand(KumandaKodlari.MoveStepLeft);
        }
    }
}
