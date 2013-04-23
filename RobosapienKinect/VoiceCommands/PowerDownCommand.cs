namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class PowerDownCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "power down";

        public PowerDownCommand() : base(COMMAND_NAME) {}

        public override void Execute() {
            TransmitCommand(KumandaKodlari.PowerOff);
        }
    }
}