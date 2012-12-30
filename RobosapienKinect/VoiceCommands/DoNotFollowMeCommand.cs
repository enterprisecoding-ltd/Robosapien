namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class DoNotFollowMeCommand: VoiceCommandBase {
        public const string COMMAND_NAME = "do not fellow me";

        public DoNotFollowMeCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            RoboManager.Instance.FollowUp = false;
        }
    }
}
