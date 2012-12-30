using UsbUirt;
using System;

namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal abstract class VoiceCommandBase {
        private Controller usbuirtController = new Controller();

        public string Command { get; private set; }

        public VoiceCommandBase(string command) {
            Command = command;
        }

        public abstract void Execute();

        protected void TransmitCommand(string command) {
            usbuirtController.Transmit(command, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }
    }
}
