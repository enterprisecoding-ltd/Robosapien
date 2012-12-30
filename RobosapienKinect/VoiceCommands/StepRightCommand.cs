namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class StepRightCommand: VoiceCommandBase {
        public const string COMMAND_NAME = "step right";

        public StepRightCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            TransmitCommand(KumandaKodlari.MoveStepRight);
        }
    }
}
