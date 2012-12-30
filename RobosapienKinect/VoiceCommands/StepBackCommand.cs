namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands
{
    internal sealed class StepBackCommand: VoiceCommandBase {
        public const string COMMAND_NAME = "step back";

        public StepBackCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            TransmitCommand(KumandaKodlari.MoveStepBack);
        }
    }
}
