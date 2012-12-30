using System;

namespace UsbUirt
{
	/// <summary>
	/// Event args passed to the LearnCompleted event.
	/// </summary>
	public class LearnCompletedEventArgs : EventArgs
	{
		private Exception _error;
		private bool _cancelled;
		private string _code;
		private object _userState;

		internal LearnCompletedEventArgs(
			Exception error,
			bool cancelled,
			string code, 
			object userState
			)
		{
			_error = error;
			_cancelled = cancelled;
			_code = code;
			_userState = userState;
		}

		/// <summary>
		/// Gets any exception thrown while learning.
		/// </summary>
		public Exception Error 
		{
			get 
			{
				return _error;
			}
		}

		/// <summary>
		/// Gets whether learning was cancelled.
		/// </summary>
		public bool Cancelled
		{
			get 
			{ 
				return _cancelled;
			}
		}

		/// <summary>
		/// Gets the learned code, if learning completed successfully.
		/// </summary>
		public string Code
		{
			get 
			{ 
				if (_cancelled) 
				{
					throw new InvalidOperationException("Learning was cancelled.");
				}
				return _code;
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
