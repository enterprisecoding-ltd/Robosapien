
using System;
namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal sealed class WakeUpCommand : VoiceCommandBase {
        public const string COMMAND_NAME = "wake up";

        public WakeUpCommand() : base(COMMAND_NAME) { }

        public override void Execute() {
            TransmitCommand(KumandaKodlari.WakeUp);
        }
    }
}
