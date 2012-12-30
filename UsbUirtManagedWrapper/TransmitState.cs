using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace UsbUirt
{
	/// <summary>
	/// Summary description for TransmitState.
	/// </summary>
	internal class TransmitState : IDisposable
	{
		private string _irCode; 
		private CodeFormat _codeFormat;
		private int _repeatCount;
		private int _inactivityWaitTime;
		private ManualResetEvent _evt;
		private bool _disposed = false;
		private object _userState;

		internal TransmitState(string irCode, CodeFormat codeFormat, int repeatCount,
			int inactivityWaitTime, object userState)
		{
			_irCode = irCode;
			_codeFormat = codeFormat;
			_repeatCount = repeatCount;
			_inactivityWaitTime = inactivityWaitTime;
			_userState = userState;
			_evt = new ManualResetEvent(false);
		}

		internal string IRCode 
		{
			get 
			{
				return _irCode;
			}
		}

		internal int RepeatCount
		{
			get 
			{
				return _repeatCount;
			}
		}

		internal int InactivityWaitTime
		{
			get 
			{
				return _inactivityWaitTime;
			}
		}

		internal CodeFormat CodeFormat
		{
			get 
			{
				return _codeFormat; 
			} 
		}

		internal ManualResetEvent WaitEvent 
		{
			get 
			{ 
				return _evt;
			}
		}

		internal object UserState
		{
			get 
			{ 
				return _userState;
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if(!this._disposed)
			{
				if(disposing)
				{
					// Dispose any managed resources.
				}
             
				if (null != _evt) 
				{
					_evt.Close();
					_evt = null;
				}

			}
			_disposed = true;         
		}
		#endregion

	}
}
