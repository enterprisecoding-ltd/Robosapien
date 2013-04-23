using System;
using System.Threading;

namespace UsbUirt {
    /// <summary>
    ///     Summary description for TransmitState.
    /// </summary>
    internal class TransmitState : IDisposable {
        private readonly CodeFormat _codeFormat;
        private readonly int _inactivityWaitTime;
        private readonly string _irCode;
        private readonly int _repeatCount;
        private readonly object _userState;
        private bool _disposed;
        private ManualResetEvent _evt;

        internal TransmitState(string irCode, CodeFormat codeFormat, int repeatCount,
                               int inactivityWaitTime, object userState) {
            _irCode = irCode;
            _codeFormat = codeFormat;
            _repeatCount = repeatCount;
            _inactivityWaitTime = inactivityWaitTime;
            _userState = userState;
            _evt = new ManualResetEvent(false);
        }

        internal string IRCode {
            get { return _irCode; }
        }

        internal int RepeatCount {
            get { return _repeatCount; }
        }

        internal int InactivityWaitTime {
            get { return _inactivityWaitTime; }
        }

        internal CodeFormat CodeFormat {
            get { return _codeFormat; }
        }

        internal ManualResetEvent WaitEvent {
            get { return _evt; }
        }

        internal object UserState {
            get { return _userState; }
        }

        #region IDisposable Members

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // Dispose any managed resources.
                }

                if (null != _evt) {
                    _evt.Close();
                    _evt = null;
                }
            }
            _disposed = true;
        }

        #endregion
    }
}