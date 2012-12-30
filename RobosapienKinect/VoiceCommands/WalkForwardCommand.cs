namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class WalkForwardCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "walk forward";

        public WalkForwardCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            TransmitCommand(KumandaKodlari.MoveWalkForward);
        }
    }
}