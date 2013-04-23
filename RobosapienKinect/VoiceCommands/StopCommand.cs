namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class StopCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "stop";

        public StopCommand() : base(COMMAND_NAME) {}

        public override void Execute() {
            TransmitCommand(KumandaKodlari.Stop);
        }
    }
}