using System;

namespace UsbUirt
{
	/// <summary>
	/// Event args passed to the TransmitCompleted event.
	/// </summary>
	public class TransmitCompletedEventArgs : EventArgs
	{
		private Exception _error;
		private object _userState;

		internal TransmitCompletedEventArgs(
			Exception error,
			object userState
			)
		{
			_error = error;
			_userState = userState;
		}

		/// <summary>
		/// Gets any exception thrown while transmitting.
		/// </summary>
		public Exception Error 
		{
			get 
			{
				return _error;
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
