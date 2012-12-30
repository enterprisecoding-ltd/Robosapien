namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class DanceCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "dance";

        public DanceCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            TransmitCommand(KumandaKodlari.Dance);
        }
    }
}
