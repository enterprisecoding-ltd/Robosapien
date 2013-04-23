namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class WalkRightCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "walk right";

        public WalkRightCommand() : base(COMMAND_NAME) {}

        public override void Execute() {
            TransmitCommand(KumandaKodlari.MoveWalkRight);
        }
    }
}