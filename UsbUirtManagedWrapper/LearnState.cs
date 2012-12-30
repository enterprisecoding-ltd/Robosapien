using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace UsbUirt
{
	/// <summary>
	/// Summary description for LearnState.
	/// </summary>
	internal class LearnState : IDisposable
	{
		private uint _forcedFrequency = 0;
		private bool _disposed = false;
		private object _userState;
		private CodeFormat _codeFormat;
		private LearnCodeModifier _learnCodeFormat;
		private  IntPtr _abort;

		internal LearnState(CodeFormat codeFormat, LearnCodeModifier learnCodeFormat, 
			uint forcedFrequency, object userState)
		{
			_codeFormat = codeFormat;
			_learnCodeFormat = learnCodeFormat;
			_forcedFrequency = forcedFrequency;
			_userState = userState;
			_abort = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Int32)));
			Marshal.WriteInt32(_abort, 0);
		}

		internal CodeFormat CodeFormat
		{
			get 
			{
				return _codeFormat; 
			} 
		}

		internal LearnCodeModifier LearnCodeModifier 
		{
			get
			{
				return _learnCodeFormat;
			}
		}

		internal uint ForcedFrequency
		{
			get
			{
				return _forcedFrequency;
			}
		}

		internal object UserState
		{
			get 
			{
				return _userState; 
			} 
		}

		internal IntPtr AbortFlag 
		{
			get
			{
				return _abort;
			}
		}

		internal bool WasAborted 
		{
			get 
			{
				return Marshal.ReadInt32(_abort) != 0;
			}
		}

		internal void Abort() 
		{
			Marshal.WriteInt32(_abort, 1);
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
             
				if (IntPtr.Zero != _abort) 
				{
					Marshal.FreeHGlobal(_abort);
					_abort = IntPtr.Zero;
				}

			}
			_disposed = true;         
		}
		#endregion

	}
}
