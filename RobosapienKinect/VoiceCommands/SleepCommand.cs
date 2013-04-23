namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class SleepCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "sleep";

        public SleepCommand() : base(COMMAND_NAME) {}

        public override void Execute() {
            TransmitCommand(KumandaKodlari.Sleep);
        }
    }
}