namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class WalkLeftCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "walk left";

        public WalkLeftCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            TransmitCommand(KumandaKodlari.MoveWalkLeft);
        }
    }
}
