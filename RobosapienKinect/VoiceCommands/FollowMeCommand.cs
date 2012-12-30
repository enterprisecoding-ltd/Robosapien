namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class FollowMeCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "fellow me";

        public FollowMeCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            RoboManager.Instance.FollowUp = true;
        }
    }
}
