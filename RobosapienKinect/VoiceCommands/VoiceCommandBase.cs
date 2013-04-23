using System;
using UsbUirt;

namespace Com.Enterprisecoding.RobosapienKinect.VoiceCommands {
    internal abstract class VoiceCommandBase {
        private readonly Controller usbuirtController = new Controller();

        public VoiceCommandBase(string command) {
            Command = command;
        }

        public string Command { get; private set; }

        public abstract void Execute();

        protected void TransmitCommand(string command) {
            usbuirtController.Transmit(command, KumandaKodlari.KodFormati, 1, TimeSpan.Zero);
        }
    }
}