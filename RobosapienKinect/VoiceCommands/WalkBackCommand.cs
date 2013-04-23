namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class WalkBackCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "walk back";

        public WalkBackCommand() : base(COMMAND_NAME) {}

        public override void Execute() {
            TransmitCommand(KumandaKodlari.MoveWalkBack);
        }
    }
}