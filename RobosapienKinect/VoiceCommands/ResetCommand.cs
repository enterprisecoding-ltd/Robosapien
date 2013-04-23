namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class ResetCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "reset";

        public ResetCommand() : base(COMMAND_NAME) {}

        public override void Execute() {
            TransmitCommand(KumandaKodlari.Reset);
        }
    }
}