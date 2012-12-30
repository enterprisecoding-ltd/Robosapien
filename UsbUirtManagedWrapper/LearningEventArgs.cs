using System;

namespace UsbUirt
{
	/// <summary>
	/// Event args passed to the Learning event.
	/// </summary>
	public class LearningEventArgs : EventArgs
	{
		private uint _progress;
		private uint _sigQuality;
		private uint _carrierFreq;
		private object _userState;

		internal LearningEventArgs(uint progress, 
			uint  sigQuality,
			uint  carrierFreq, 
			object userState
			)
		{
			_progress = progress;
			_sigQuality = sigQuality;
			_carrierFreq = carrierFreq;
			_userState = userState;
		}

		/// <summary>
		/// Gets the progress, from 0 to 100.
		/// </summary>
		public int Progress 
		{
			get 
			{ 
				return Convert.ToInt32(_progress);
			}
		}

		/// <summary>
		/// Gets the currently detected signal quality.
		/// </summary>
		public int SignalQuality 
		{
			get 
			{ 
				return Convert.ToInt32(_sigQuality);
			}
		}

		/// <summary>
		/// Gets the currently detected carrier frequency.
		/// </summary>
		public int CarrierFrequency 
		{
			get 
			{ 
				return Convert.ToInt32(_carrierFreq);
			}
		}

		/// <summary>
		/// Gets the optional user state.
		/// </summary>
		public object UserState
		{
			get 
			{
				return _userState; 
			}
		}
	}
}
