using System;
using System.Threading;

namespace UsbUirt
{
	/// <summary>
	/// Summary description for SyncLearnResults.
	/// </summary>
	internal class SyncLearnResults : IDisposable
	{
		private bool _disposed = false;
		private ManualResetEvent _manualResetEvent;
		private LearnCompletedEventArgs _learnedEventArgs;

		internal SyncLearnResults() 
		{
			_manualResetEvent = new ManualResetEvent(false);
		}

		internal ManualResetEvent WaitEvent 
		{
			get 
			{
				return _manualResetEvent; 
			} 
		}

		internal LearnCompletedEventArgs LearnCompletedEventArgs 
		{
			get 
			{
				return _learnedEventArgs; 
			} 
			set 
			{
				_learnedEventArgs = value; 
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
             
				if (null != _manualResetEvent) 
				{
					_manualResetEvent.Close();
					_manualResetEvent = null;
				}

			}
			_disposed = true;         
		}
		#endregion
	}
}
