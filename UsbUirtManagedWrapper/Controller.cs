using System;
using System.Collections;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace UsbUirt
{
	/// <summary>
	/// Provides a managed wrapper around the USB-UIRT driver.
	/// </summary>
	public sealed class Controller : IDisposable
	{
		#region Fields
		private static CodeFormat _defaultLearnCodeFormat = CodeFormat.Pronto;
		private static LearnCodeModifier _defaultLearnCodeModifier = LearnCodeModifier.None;
		private static CodeFormat _defaultTransmitCodeFormat = CodeFormat.Pronto;
		private const int _defaultRepeatCount = 1;
		private static TimeSpan _defaultInactivityWaitTime = TimeSpan.FromMilliseconds(50);
		private static uint _drvVersion;

		private IntPtr _hDrvHandle = IntPtr.Zero;
		private bool _disposed = false;
		private Hashtable _learnStates;

		private ReceivedEventHandler _received;
		private LearningEventHandler _learning;
		private LearnCompletedEventHandler _learnCompleted;
		private TransmitCompletedEventHandler _transmitCompleted;
		private object _receivedSyncRoot = new object();
		private object _learningSyncRoot = new object();
		private object _learnCompletedSyncRoot = new object();
		private object _transmitCompletedSyncRoot = new object();

		private DateTime _firmwareDate;
		private uint _firmwareVersion;
		private uint _protocolVersion;

		private ReceiveCallback myCb;
		#endregion

		static Controller()
		{
			try 
			{
				if (UUIRTGetDrvInfo(out _drvVersion) == false) 
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}
			} 
			catch (Exception ex) 
			{
				throw new ApplicationException("Unable to read UsbUirt driver version", ex);
			}
		}

		/// <summary>
		/// Creates a new UsbUirt Controller.
		/// </summary>
		/// <remarks>In order to ensure that the connection with the USBUirt device is closed,
		/// you must call Dispose() when you are done with object.</remarks>
		public Controller()
		{
			_learnStates = new Hashtable();
			_hDrvHandle = OpenDriver();

			UUINFO uuInfo = GetVersion();
			_firmwareDate = new DateTime(2000 + uuInfo.fwDateYear, uuInfo.fwDateMonth, uuInfo.fwDateDay);
			_firmwareVersion = uuInfo.fwVersion;
			_protocolVersion = uuInfo.protVersion;

#if false
			myCb = new ReceiveCallback(ReceiveCallbackProc);
			SetReceiveCallback(myCb);
#endif
		}

		/// <summary>
		/// If the author forgets to call Dispose, free unmanaged resources at finalization.
		/// </summary>
		~Controller() 
		{
//			GC.KeepAlive(myCb);
			Dispose(false);
		}

		#region Public properties
		/// <summary>
		/// Gets the version of the UsbUirt driver installed.
		/// </summary>
		public static uint DriverVersion 
		{
			get 
			{
				return _drvVersion;
			}
		}

		/// <summary>
		/// Gets the version of the detected UsbUirt firmware.
		/// </summary>
		public uint FirmwareVersion 
		{ 
			get 
			{ 
				return _firmwareVersion; 
			}
		}

		/// <summary>
		/// Gets the release date of the detected UsbUirt firmware.
		/// </summary>
		public DateTime FirmwareDate
		{ 
			get 
			{
				return _firmwareDate; 
			}
		}

		/// <summary>
		/// Gets the protocol of the detected UsbUirt firmware.
		/// </summary>
		public uint ProtocolVersion 
		{ 
			get 
			{ 
				return _protocolVersion; 
			}
		}

		/// <summary>
		/// Gets or sets whether the UsbUirt should blink when IR is received.
		/// </summary>
		/// <value>True if the UsbUirt should blink when IR is received.</value>
		public bool BlinkOnReceive 
		{
			get 
			{ 
				CheckDisposed();
				return (GetConfig() & UUIRTConfigBits.BlinkOnReceive) != 0; 
			}
			set 
			{
				CheckDisposed();
				SetConfig(value, BlinkOnTransmit, GenerateLegacyCodesOnReceive);
			}
		}

		/// <summary>
		/// Gets or sets whether the UsbUirt should blink when IR is transmitted.
		/// </summary>
		/// <value>True if the UsbUirt should blink when IR is transmitted.</value>
		public bool BlinkOnTransmit 
		{
			get 
			{ 
				CheckDisposed();
				return (GetConfig() & UUIRTConfigBits.BlinkOnTransmit) != 0; 
			}
			set 
			{
				CheckDisposed();
				SetConfig(BlinkOnReceive, value, GenerateLegacyCodesOnReceive);
			}
		}

		/// <summary>
		/// Gets or sets whether the UsbUirt should generate legacy codes when IR is received.
		/// </summary>
		/// <value>True if the UsbUirt should generate legacy codes when IR is received.</value>
		public bool GenerateLegacyCodesOnReceive 
		{
			get 
			{ 
				CheckDisposed();
				return (GetConfig() & UUIRTConfigBits.GenerateLegacyCodesOnReceive) != 0;
			}
			set 
			{
				CheckDisposed();
				SetConfig(BlinkOnReceive, BlinkOnTransmit, value);
			}
		}
		#endregion

		#region Public delegates and events
		/// <summary>
		/// The delegate used for the Received event.
		/// </summary>
		public delegate void ReceivedEventHandler(object sender, ReceivedEventArgs e);

		/// <summary>
		/// The delegate used for the Learning event.
		/// </summary>
		public delegate void LearningEventHandler(object sender, LearningEventArgs e);

		/// <summary>
		/// The delegate used for the LarnCompleted event.
		/// </summary>
		public delegate void LearnCompletedEventHandler(object sender, LearnCompletedEventArgs e);

		/// <summary>
		/// The delegate used for the TransmitCompleted event.
		/// </summary>
		public delegate void TransmitCompletedEventHandler(object sender, TransmitCompletedEventArgs e);
		
		/// <summary>
		/// Raised when IR input is received.
		/// </summary>
		/// 
		public event ReceivedEventHandler Received
		{
			add 
			{
				CheckDisposed();
				lock (_receivedSyncRoot) 
				{
					if (null == _received) 
					{
						myCb = new ReceiveCallback(ReceiveCallbackProc);
						SetReceiveCallback(myCb);
					}
					_received += value;         
				} 
			}
			remove 
			{
				CheckDisposed();
				lock (_receivedSyncRoot) 
				{
					_received -= value;
					if (_received == null) 
					{
						ClearReceiveCallback();
						GC.KeepAlive(myCb);
					}
				}
			}
		}

		/// <summary>
		/// Raised periodically during learning, to provided feedback on progress.
		/// </summary>
		public event LearningEventHandler Learning
		{
			add 
			{
				CheckDisposed();
				lock (_learningSyncRoot) 
				{
					_learning += value;         
				} 
			}
			remove 
			{
				CheckDisposed();
				lock (_learningSyncRoot) 
				{
					_learning -= value;
				}
			}
		}

		/// <summary>
		/// Raised when learning, begun via LearnAsync(), has completed.
		/// </summary>
		public event LearnCompletedEventHandler LearnCompleted
		{
			add 
			{
				CheckDisposed();
				lock (_learnCompletedSyncRoot) 
				{
					_learnCompleted += value;         
				} 
			}
			remove 
			{
				CheckDisposed();
				lock (_learnCompletedSyncRoot) 
				{
					_learnCompleted -= value;
				}
			}
		}

		/// <summary>
		/// Raised when transmission, begun via TransmitAsync(), has completed.
		/// </summary>
		public event TransmitCompletedEventHandler TransmitCompleted
		{
			add 
			{
				CheckDisposed();
				lock (_transmitCompletedSyncRoot) 
				{
					_transmitCompleted += value;         
				} 
			}
			remove 
			{
				CheckDisposed();
				lock (_transmitCompletedSyncRoot) 
				{
					_transmitCompleted -= value;
				}
			}
		}
		#endregion

		#region Public methods

		/// <summary>
		/// Releases all managed and unmanaged resources held by this object.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#region Transmit overloads
		/// <summary>
		/// Transmits an IR code synchronously using the default code format.
		/// </summary>
		/// <param name="irCode">The IR code to transmit.</param>
		public void Transmit(string irCode) 
		{
			CheckDisposed();
			Transmit(irCode, _defaultTransmitCodeFormat, _defaultRepeatCount, _defaultInactivityWaitTime);
		}

		/// <summary>
		/// Transmits an IR code synchronously.
		/// </summary>
		/// <param name="irCode">The IR code to transmit.</param>
		/// <param name="codeFormat">The format of the IR code.</param>
		/// <param name="repeatCount">Indicates how many iterations of the code should be 
		/// sent (in the case of a 2-piece code, the first stream is sent once followed 
		/// by the second stream sent repeatCount times).</param>
		/// <param name="inactivityWaitTime">Time in milliseconds since the last received 
		/// IR activity to wait before sending an IR code. Normally, pass 0 for this parameter.</param>
		public void Transmit(
			string irCode, 
			CodeFormat codeFormat, 
			int repeatCount,
			TimeSpan inactivityWaitTime) 
		{
			CheckDisposed();
			if (null == irCode) 
			{
				throw new ArgumentNullException("irCode", "irCode cannot be null");
			}

			if (0 == irCode.Length) 
			{
				throw new ArgumentException("irCode", "irCode cannot be empty");
			}

			if (repeatCount < 0) 
			{
				throw new ArgumentOutOfRangeException("repeatCount", "repeatCount cannot be negative");
			}

			if (inactivityWaitTime < TimeSpan.Zero) 
			{
				throw new ArgumentOutOfRangeException("inactivityWaitTime", "inactivityWaitTime cannot be less than TimeSpan.Zero");
			}

			using (ManualResetEvent evt = new ManualResetEvent(false)) 
			{
				TransmitIr(irCode, codeFormat, repeatCount, Convert.ToInt32(inactivityWaitTime.TotalMilliseconds), evt);
				evt.WaitOne();		
			}
		}

		/// <summary>
		/// Transmits an IR code asynchronously using the default code format.
		/// </summary>
		/// <param name="irCode">The IR code to transmit.</param>
		public void TransmitAsync(string irCode) 
		{
			CheckDisposed();
			TransmitAsync(irCode, _defaultTransmitCodeFormat, _defaultRepeatCount, _defaultInactivityWaitTime, null);
		}

		/// <summary>
		/// Transmits an IR code asynchronously.
		/// </summary>
		/// <param name="irCode">The IR code to transmit.</param>
		/// <param name="codeFormat">The format of the IR code.</param>
		/// <param name="repeatCount">Indicates how many iterations of the code should be 
		/// sent (in the case of a 2-piece code, the first stream is sent once followed 
		/// by the second stream sent repeatCount times).</param>
		/// <param name="inactivityWaitTime">Time in milliseconds since the last received 
		/// IR activity to wait before sending an IR code. Normally, pass 0 for this parameter.</param>
		public void TransmitAsync(
			string irCode, 
			CodeFormat codeFormat, 
			int repeatCount,
			TimeSpan inactivityWaitTime) 
		{
			CheckDisposed();
			TransmitAsync(irCode, codeFormat, repeatCount, inactivityWaitTime, null);
		}
		
		/// <summary>
		/// Transmits an IR code asynchronously.
		/// </summary>
		/// <param name="irCode">The IR code to transmit.</param>
		/// <param name="codeFormat">The format of the IR code.</param>
		/// <param name="repeatCount">Indicates how many iterations of the code should be 
		/// sent (in the case of a 2-piece code, the first stream is sent once followed 
		/// by the second stream sent repeatCount times).</param>
		/// <param name="inactivityWaitTime">Time in milliseconds since the last received 
		/// IR activity to wait before sending an IR code. Normally, pass 0 for this parameter.</param>
		/// <param name="userState">An optional user state object that will be passed to the 
		/// TransmitCompleted event.</param>
		public void TransmitAsync(
			string irCode, 
			CodeFormat codeFormat, 
			int repeatCount,
			TimeSpan inactivityWaitTime,
			object userState) 
		{
			CheckDisposed();
			if (null == irCode) 
			{
				throw new ArgumentNullException("irCode", "irCode cannot be null");
			}

			if (0 == irCode.Length) 
			{
				throw new ArgumentException("irCode", "irCode cannot be empty");
			}

			if (repeatCount < 0) 
			{
				throw new ArgumentOutOfRangeException("repeatCount", "repeatCount cannot be negative");
			}

			if (inactivityWaitTime < TimeSpan.Zero) 
			{
				throw new ArgumentOutOfRangeException("inactivityWaitTime", "inactivityWaitTime cannot be less than TimeSpan.Zero");
			}

			TransmitState transmitState = new TransmitState(irCode, codeFormat, repeatCount, Convert.ToInt32(inactivityWaitTime.TotalMilliseconds), userState);
			if (false == ThreadPool.QueueUserWorkItem(new WaitCallback(DoTransmit), transmitState))
			{
				throw new ApplicationException("Unable to QueueUserWorkItem");
			}
		}
		#endregion

		#region Learn and LearnCancel overloads
		/// <summary>
		/// Learns an IR code synchronously using the default code format.
		/// </summary>
		/// <returns>The IR code that was learned, or null if learning failed.</returns>
		public string Learn() 
		{
			CheckDisposed();
			return Learn(_defaultLearnCodeFormat, _defaultLearnCodeModifier, TimeSpan.Zero);
		}

		/// <summary>
		/// Learns an IR code synchronously.
		/// </summary>
		/// <param name="codeFormat">The format of the IR code to use in learning.</param>
		/// <returns>The IR code that was learned, or null if learning failed.</returns>
		public string Learn(CodeFormat codeFormat) 
		{
			CheckDisposed();
			return Learn(codeFormat, _defaultLearnCodeModifier, TimeSpan.Zero);
		}

		/// <summary>
		/// Learns an IR code synchronously using the default code format.
		/// </summary>
		/// <param name="timeout">The timeout after which to abort learning if it has not completed.</param>
		/// <returns>The IR code that was learned, or null if learning failed.</returns>
		public string Learn(TimeSpan timeout) 
		{
			CheckDisposed();
			return Learn(_defaultLearnCodeFormat, _defaultLearnCodeModifier, timeout);
		}

		/// <summary>
		/// Learns an IR code synchronously.
		/// </summary>
		/// <param name="codeFormat">The format of the IR code to use in learning.</param>
		/// <param name="learnCodeFormat">The modifier used for the code format.</param>
		/// <param name="timeout">The timeout after which to abort learning if it has not completed.</param>
		/// <returns>The IR code that was learned, or null if learning failed.</returns>
		public string Learn(CodeFormat codeFormat, LearnCodeModifier learnCodeFormat, TimeSpan timeout) 
		{
			CheckDisposed();
			return  Learn(codeFormat, learnCodeFormat, 0, timeout);
		}

		/// <summary>
		/// Learns an IR code synchronously.
		/// </summary>
		/// <param name="codeFormat">The format of the IR code to use in learning.</param>
		/// <param name="learnCodeFormat">The modifier used for the code format.</param>
		/// <param name="forcedFrequency">The frequency to use in learning.</param>
		/// <param name="timeout">The timeout after which to abort learning if it has not completed.</param>
		/// <returns>The IR code that was learned, or null if learning failed.</returns>
		public string Learn(CodeFormat codeFormat, LearnCodeModifier learnCodeFormat, 
			uint forcedFrequency, TimeSpan timeout) 
		{
			CheckDisposed();
			if (timeout < TimeSpan.Zero) 
			{
				throw new ArgumentOutOfRangeException("timeout", "timeout cannot be negative");
			}

			using (SyncLearnResults results = new SyncLearnResults()) 
			{
				this.LearnCompleted += new LearnCompletedEventHandler(ManagedWrapper_LearnCompleted);

				try 
				{
					LearnAsync(codeFormat, learnCodeFormat, forcedFrequency, results);
					if (TimeSpan.Zero == timeout) 
					{
						results.WaitEvent.WaitOne();
						return results.LearnCompletedEventArgs.Code;
					} 
					else if (results.WaitEvent.WaitOne(timeout, false)) 
					{
						if (null != results.LearnCompletedEventArgs.Error) 
						{
							throw results.LearnCompletedEventArgs.Error;
						} 
						else if (false != results.LearnCompletedEventArgs.Cancelled) 
						{
							return null;
						}
						return results.LearnCompletedEventArgs.Code;
					} 
					else 
					{
						LearnAsyncCancel(results);
						return null;
					}
				} 
				finally 
				{
					this.LearnCompleted -= new LearnCompletedEventHandler(ManagedWrapper_LearnCompleted);
				}			
			}
		}
	
		/// <summary>
		/// Learns an IR code asynchronously using the default code format.
		/// </summary>
		public void LearnAsync() 
		{
			CheckDisposed();
			LearnAsync(_defaultLearnCodeFormat, _defaultLearnCodeModifier, null);
		}

		/// <summary>
		/// Learns an IR code asynchronously.
		/// </summary>
		/// <param name="codeFormat">The format of the IR code to use in learning.</param>
		public void LearnAsync(CodeFormat codeFormat) 
		{
			CheckDisposed();
			LearnAsync(codeFormat, _defaultLearnCodeModifier, null);
		}

		/// <summary>
		/// Learns an IR code asynchronously.
		/// </summary>
		/// <param name="codeFormat">The format of the IR code to use in learning.</param>
		/// <param name="learnCodeFormat">The modifier used for the code format.</param>
		/// <param name="userState">An optional user state object that will be passed to the 
		/// Learning and LearnCompleted events and which can be used when calling LearnAsyncCancel().</param>
		public void LearnAsync(CodeFormat codeFormat, LearnCodeModifier learnCodeFormat, object userState) 
		{
			CheckDisposed();
			LearnAsync(codeFormat, learnCodeFormat, 0, userState);
		}

		/// <summary>
		/// Learns an IR code asynchronously.
		/// </summary>
		/// <param name="codeFormat">The format of the IR code to use in learning.</param>
		/// <param name="learnCodeFormat">The modifier used for the code format.</param>
		/// <param name="forcedFrequency">The frequency to use in learning.</param>
		/// <param name="userState">An optional user state object that will be passed to the 
		/// Learning and LearnCompleted events and which can be used when calling LearnAsyncCancel().</param>
		public void LearnAsync(CodeFormat codeFormat, LearnCodeModifier learnCodeFormat,
			uint forcedFrequency, object userState) 
		{
			CheckDisposed();
			if (LearnCodeModifier.ForceFrequency == learnCodeFormat) 
			{
				if (0 == forcedFrequency) 
				{
					throw new ArgumentException("forcedFrequency", "forcedFrequency must be specified when using LearnCodeModifier.ForceFrequency");
				}
			} 
			else 
			{
				if (0 != forcedFrequency) 
				{
					throw new ArgumentException("forcedFrequency", "forcedFrequency can only be specified when using LearnCodeModifier.ForceFrequency");
				}
			}

			object learnStatesKey = null == userState ? this : userState;
			LearnState learnState = new LearnState(codeFormat, learnCodeFormat, forcedFrequency, userState);
			_learnStates[learnStatesKey] = learnState;
				
			if (false == ThreadPool.QueueUserWorkItem(new WaitCallback(DoLearn), learnState))
			{
				throw new ApplicationException("Unable to QueueUserWorkItem");
			}
		}

		/// <summary>
		/// Cancels a LearnAsync() operation.
		/// </summary>
		public void LearnAsyncCancel() 
		{
			CheckDisposed();
			LearnAsyncCancel(null);
		}

		/// <summary>
		/// Cancels a LearnAsync() operation that was passed the specified userState.
		/// </summary>
		/// <param name="userState">The optional userState object passed to LearnAsync().</param>
		public bool LearnAsyncCancel(object userState) 
		{
			CheckDisposed();
			object learnStatesKey = null == userState ? this : userState;
			LearnState learnState = _learnStates[learnStatesKey] as LearnState;
			if (null != learnState) 
			{
				learnState.Abort();
				return true;
			}
			return false;
		}
		#endregion

		#endregion

		#region Private methods
		private void Dispose(bool disposeManagedResources)
		{
			if(!this._disposed)
			{
				_disposed = true;

				if(disposeManagedResources)
				{
					// Dispose any managed resources.
				}
			 
				if (IntPtr.Zero != _hDrvHandle) 
				{
					UUIRTClose(_hDrvHandle);
					_hDrvHandle = IntPtr.Zero;
				}
			}
		}

		private void CheckDisposed() 
		{
			if (_disposed) 
			{
				throw new ObjectDisposedException("Controller", "The UsbUirt.Controller has already been disposed");
			}
		}

		private void DoTransmit(object state) 
		{
			TransmitState transmitState = state as TransmitState;
			try 
			{
				Exception error = null;
				try 
				{
					TransmitIr(
						transmitState.IRCode, 
						transmitState.CodeFormat, 
						transmitState.RepeatCount, 
						transmitState.InactivityWaitTime, 
						transmitState.WaitEvent);
					transmitState.WaitEvent.WaitOne();
				} 
				catch (Exception ex) 
				{
					error = ex;
				}

				TransmitCompletedEventHandler temp = _transmitCompleted;
				if (null != temp) 
				{
					temp(this, new TransmitCompletedEventArgs(error, transmitState.UserState));
				}
			} 
			finally 
			{
				transmitState.Dispose();
			}
		}

		private void DoLearn(object state) 
		{
			LearnState learnState = state as LearnState;
			StringBuilder outCode = new StringBuilder(4096);
			GCHandle userDataHandle = new GCHandle();
			IntPtr userDataPtr = IntPtr.Zero;

			try 
			{
				userDataHandle = GCHandle.Alloc(learnState);
				userDataPtr = (IntPtr) userDataHandle;
				Exception error = null;

				if (false == UUIRTLearnIR(
					_hDrvHandle, 
					(int) learnState.CodeFormat | (int) learnState.LearnCodeModifier,
					outCode,
					new LearnCallback(LearnCallbackProc),
					userDataPtr,
					learnState.AbortFlag,
					learnState.ForcedFrequency, IntPtr.Zero, IntPtr.Zero)) 
				{
					try 
					{
						Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
					} 
					catch (Exception ex) 
					{
						error = ex;
					}
				}

				if (userDataHandle.IsAllocated) 
				{
					userDataHandle.Free();
				}

				LearnCompletedEventHandler temp = _learnCompleted;
				if (null != temp) 
				{
					temp(this, new LearnCompletedEventArgs(error, learnState.WasAborted, outCode.ToString(), learnState.UserState));
				}
			} 
			finally 
			{
				learnState.Dispose();
				object learnStatesKey = null == learnState.UserState ? this : state;
				_learnStates[learnStatesKey] = null;
			}
		}
		
		private void ManagedWrapper_LearnCompleted(object sender, LearnCompletedEventArgs e)
		{
			SyncLearnResults syncLearnResults = e.UserState as SyncLearnResults;
			if (null == syncLearnResults) 
			{
				throw new ApplicationException("invalid userState received");
			}
			syncLearnResults.LearnCompletedEventArgs = e;
			syncLearnResults.WaitEvent.Set();
		}

		private void ReceiveCallbackProc(
			StringBuilder irEventString,
			IntPtr userState) 
		{
			ReceivedEventHandler temp = _received;
			if (null != temp) 
			{	
				temp(this, new ReceivedEventArgs(irEventString.ToString()));
			}
		}

		private void LearnCallbackProc(
			uint progress, 
			uint sigQuality,
			uint carrierFreq,
			IntPtr userState) 
		{
			GCHandle userDataHandle = (GCHandle) userState;
			object state = userDataHandle.Target;
			LearningEventHandler temp = _learning;
			if (null != temp) 
			{
				temp(this, new LearningEventArgs(progress, sigQuality, carrierFreq, state));
			}
		}

		private void SetReceiveCallback(ReceiveCallback cb) 
		{
			if (false == UUIRTSetReceiveCallback(_hDrvHandle, cb/*new ReceiveCallback(ReceiveCallbackProc)*/, IntPtr.Zero)){
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
//JLR Test
//			GC.Collect();
//			GC.WaitForPendingFinalizers();
//			GC.Collect();
//JLR Test
		}

		private void ClearReceiveCallback() 
		{
			if (false == UUIRTSetReceiveCallback(_hDrvHandle, null, IntPtr.Zero))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}

		private IntPtr OpenDriver() 
		{
			IntPtr hDrvHandle = UUIRTOpen();
//	hDrvHandle = (IntPtr)INVALID_HANDLE_VALUE;
			if (hDrvHandle.ToInt32() == INVALID_HANDLE_VALUE) 
			{
				switch(Marshal.GetLastWin32Error()) 
				{
					case (int) UuirtDriverError.NoDll:
						throw new ApplicationException("Unable to find USB-UIRT Driver. Please make sure driver is Installed");

					case (int) UuirtDriverError.NoDeviceFound:
						throw new ApplicationException("Unable to connect to USB-UIRT device! Please ensure device is connected to the computer");

					case (int) UuirtDriverError.NoResponse:
						throw new ApplicationException("Unable to communicate with USB-UIRT device! Please check connections and try again.  If you still have problems, try unplugging and reconnecting your USB-UIRT.  If problem persists, contact Technical Support");

					case (int) UuirtDriverError.WrongVersion:
						throw new ApplicationException("Your USB-UIRT's firmware is not compatible with this API DLL. Please verify you are running the latest API DLL and that you're using the latest version of USB-UIRT firmware!  If problem persists, contact Technical Support");
				}

				throw new ApplicationException("Unable to initialize USB-UIRT (unknown error)");
			}
			return hDrvHandle;
		}

		private UUINFO GetVersion() 
		{
			UUINFO uuInfo = new UUINFO();
			if (false == UUIRTGetUUIRTInfo(_hDrvHandle, ref uuInfo))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
			return uuInfo;
		}

		private UUIRTConfigBits GetConfig() 
		{
			uint uConfig;
			if (false == UUIRTGetUUIRTConfig(_hDrvHandle, out uConfig))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
			return((UUIRTConfigBits) uConfig);
		}

		private void SetConfig(bool blinkOnReceive,
			bool blinkOnTransmit,
			bool generateLegacyCodesOnReceive) 
		{
			UUIRTConfigBits uConfig = 
				(blinkOnReceive ? UUIRTConfigBits.BlinkOnReceive : 0) |
				(blinkOnTransmit ? UUIRTConfigBits.BlinkOnTransmit :0) |
				(generateLegacyCodesOnReceive ? UUIRTConfigBits.GenerateLegacyCodesOnReceive : 0);

			if (false == UUIRTSetUUIRTConfig(_hDrvHandle, (uint) uConfig))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}

		private void TransmitIr(string irCode, CodeFormat codeFormat, int repeatCount,
			int inactivityWaitTime, ManualResetEvent evt) 
		{
			if (false == UUIRTTransmitIR(
				_hDrvHandle, 
				irCode, 
				(int) codeFormat, 
				repeatCount, 
				inactivityWaitTime, 
				null == evt ? IntPtr.Zero : evt.SafeWaitHandle.DangerousGetHandle(), 
				IntPtr.Zero, IntPtr.Zero))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}

		#endregion

		#region P/Invoke definitions

		/// <summary>
		/// Opens communication with the USB-UIRT.  
		/// A call to UUIRTOpen should occur prior to any other driver function calls (with 
		/// the exception of UUIRTGetDrvInfo below).
		/// </summary>
		/// <returns>On success, a handle to be used in subsequent calls to USB-UIRT
		/// functions. On failure, INVALID_HANDLE_VALUE.</returns>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern IntPtr UUIRTOpen();

		/// <summary>
		/// Terminates communication with the USB-UIRT. Should be called prior to terminating 
		/// host program.
		/// </summary>
		/// <param name="hDrvHandle">Handle to to USB-UIRT returned by UUIRTOpen</param>
		/// <returns></returns>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern bool UUIRTClose(IntPtr hDrvHandle);

		/// <summary>
		/// Retrieves information about the driver (not the hardware itself). This is 
		/// intended to allow version control on the .DLL driver and accomodate future 
		/// changes and enhancements to the API. 
		/// </summary>
		/// <remarks>This call may be called prior to a call to UUIRTOpen.</remarks>
		/// <param name="drvVersion"></param>
		/// <returns>TRUE on success</returns>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern bool UUIRTGetDrvInfo(out uint drvVersion);

		/// <summary>
		/// Retrieves information about the UUIRT hardware.
		/// </summary>
		/// <param name="hDrvHandle">Handle to to USB-UIRT returned by UUIRTOpen</param>
		/// <param name="uuInfo">UUINFO structure that will be filled in upon success</param>
		/// <returns>TRUE on success</returns>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern bool UUIRTGetUUIRTInfo(IntPtr hDrvHandle, ref UUINFO uuInfo);

		/// <summary>
		/// Reperesents information about the UUIRT hardware.
		/// </summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct UUINFO
		{
			/// <summary>
			/// Version of firmware residing on the USB-UIRT.
			/// </summary>
			internal uint fwVersion;
			/// <summary>
			/// Protocol version supported by the USB-UIRT firmware.
			/// </summary>
			internal uint protVersion;
			/// <summary>
			/// Firmware revision day
			/// </summary>
			internal byte fwDateDay;
			/// <summary>
			/// Firmware revision month
			/// </summary>
			internal byte  fwDateMonth;
			/// <summary>
			/// Firmware revision year
			/// </summary>
			internal byte  fwDateYear;
		}

		/// <summary>
		/// Retrieves the current feature configuration bits from the USB-UIRT's nonvolatile 
		/// configuration memory. These various configuration bits control how the USB-UIRT 
		/// behaves. Most are reserved for future implementation and should be read and 
		/// written as Zero.
		/// </summary>
		/// <param name="hDrvHandle">Handle to to USB-UIRT returned by UUIRTOpen</param>
		/// <param name="uConfig">Integer representation of USB-UIRT configuration</param>
		/// <returns>TRUE on success</returns>
		/// <remarks> Using this API call is optional and is only needed to support 
		/// changing USB-UIRT's private preferences</remarks>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern bool UUIRTGetUUIRTConfig(IntPtr hDrvHandle, out uint uConfig);

		/// <summary>
		/// Flags used when getting or setting the USB-UIRT configuration
		/// </summary>
		[FlagsAttribute()]
		private enum UUIRTConfigBits : uint
		{
			/// <summary>
			/// Indicator LED on USB-UIRT blinks when remote signals are received
			/// </summary>
			BlinkOnReceive = 0x01,
			/// <summary>
			/// Indicator LED on USB-UIRT lights during IR transmission.
			/// </summary>
			BlinkOnTransmit = 0x02,
			/// <summary>
			/// Generate 'legacy' UIRT-compatible codes on receive
			/// </summary>
			GenerateLegacyCodesOnReceive = 0x04,
			/// <summary>
			/// Reserved
			/// </summary>
			Reserved0 = 0x08,
			/// <summary>
			/// Reserved
			/// </summary>
			Reserved1 = 0x10
		}

		/// <summary>
		/// Configures the current feature configuration bits for the USB-UIRT's nonvolatile 
		/// configuration memory. These various configuration bits control how the USB-UIRT 
		/// behaves.
		/// </summary>
		/// <param name="hDrvHandle">Handle to to USB-UIRT returned by UUIRTOpen</param>
		/// <param name="uConfig">Integer representation of USB-UIRT configuration</param>
		/// <returns>TRUE on success</returns>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern bool UUIRTSetUUIRTConfig(IntPtr hDrvHandle, uint uConfig);

		/// <summary>
		/// Transmits an IR code via the USB-UIRT hardware.
		/// </summary>
		/// <param name="hDrvHandle">Handle to to USB-UIRT returned by UUIRTOpen</param>
		/// <param name="irCode">null-terminated string</param>
		/// <param name="codeFormat">format specifier which identifies the format of the IRCode
		/// code. Currently, supported formats are Compressed_UIRT (STRUCT), RAW, and 
		/// Pronto-RAW</param>
		/// <param name="repeatCount">indicates how many iterations of the code should be 
		/// sent (in the case of a 2-piece code, the first stream is sent once followed 
		/// by the second stream sent repeatCount times)</param>
		/// <param name="inactivityWaitTime">time 
		/// in milliseconds since the last received IR activity to wait before sending an 
		/// IR code -- normally pass 0 for this parameter</param>
		/// <param name="hEvent">optional event handle which is obtained by a call to 
		/// CreateEvent. If hEvent is NULL, the call to UUIRTTransmitIR will block and not 
		/// return until the IR code has been fully transmitted to the air. If hEvent 
		/// is not NULL, it must be a valid Windows event hande. In this case, 
		/// UUIRTTransmitIR will return immediately and when the IR stream has 
		/// completed transmission this event will be signalled by the driver</param>
		/// <param name="reserved0">reserved for future expansion; should be NULL</param>
		/// <param name="reserved1">reserved for future expansion; should be NULL</param>
		/// <returns>TRUE on success</returns>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern bool UUIRTTransmitIR(
			IntPtr hDrvHandle, 
			string irCode,
			int codeFormat,
			int repeatCount, 
			int inactivityWaitTime, 
			IntPtr hEvent,
			IntPtr reserved0,
			IntPtr reserved1);

		/// <summary>
		/// Instructs the USB-UIRT and the API to learn an IR code.
		/// </summary>
		/// <param name="hDrvHandle">Handle to to USB-UIRT returned by UUIRTOpen</param>
		/// <param name="codeFormat">format specifier which identifies the format of the IRCode
		/// code to learn. Currently, supported formats are Compressed_UIRT (STRUCT), RAW, and 
		/// Pronto-RAW</param>
		/// <param name="IRCode">the learned IR code (upon return). It is the responsibility 
		/// of the caller to allocate space for this string; suggested string size is at 
		/// least 2048 bytes</param>
		/// <param name="progressProc">a caller-supplied callback function which will be called
		/// periodically during the learn process and may be used to update user dialogs, 
		/// etc. Information passed to the callback are learn progress %, signal quality, and 
		/// carrier frequency.</param>
		/// <param name="userData">will be passed by the USB-UIRT driver to any calls of 
		/// progressProc. </param>
		/// <param name="pAbort">pointer to a Boolean variable 
		/// which should be initialized to FALSE (0) prior to the call. Setting this variable 
		/// TRUE during the learn process will cause the UUIRTLearnIR process to abort and the 
		/// function to return. Since the UUIRTLearnIR function will block for the duration of 
		/// the learn process, one could set the *pAbort to TRUE either within the callback 
		/// function or from another thread</param>
		/// <param name="param1">currently used only when the codeFormat 
		/// includes the UUIRTDRV_IRFMT_LEARN_FORCEFREQ flag (not normally needed) -- in which 
		/// case param1 should indicate the forced carrier frequency</param>
		/// <param name="reserved0">reserved for future expansion; should be NULL</param>
		/// <param name="reserved1">reserved for future expansion; should be NULL</param>
		/// <returns>TRUE on success</returns>
		/// <remarks>The IR code learned  will be a complete IR stream suitable for subsequent 
		/// transmission via UUIRTTransmitIR. Consequently, the same formats supported by 
		/// Transmit are also available for learn. It is recommended to use either RAW or 
		/// Pronto-RAW codeFormat to offer the best compatibility; compressed-UIRT format 
		/// is often too limiting, although it does produce the smallest codes.</remarks>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern bool UUIRTLearnIR(
			IntPtr hDrvHandle, 
			int codeFormat,
			StringBuilder IRCode, 
			LearnCallback progressProc, 
			IntPtr userData,
			IntPtr pAbort, 
			uint param1, 
			IntPtr reserved0, 
			IntPtr reserved1);

		/// <summary>
		/// Delegate used as a callback during learning in order to update display the progress
		/// </summary>
		private delegate void LearnCallback(
			uint progress, 
			uint  sigQuality,
			uint  carrierFreq,
			IntPtr userData);

		/// <summary>
		/// Delegate used to receive callbacks when IR input is received
		/// </summary>
		private delegate void ReceiveCallback(
			StringBuilder IREventStr,
			IntPtr userData);

		/// <summary>
		/// Registers a receive callback function which the driver will call when an IR code 
		/// is received from the air.
		/// 
		/// typedef void (WINAPI *PUUCALLBACKPROC) (char *IREventStr, void *userData);
		/// When the USB-UIRT receives a code from the air, it will call the callback function
		/// with a null-terminated, twelve-character (like IRMAN) ir code in IREventStr. 
		/// </summary>
		/// <remarks>
		/// The types of codes which are passed to IREventStr are not the same as the type
		/// of codes passed back from a UUIRTLearnIR call (the codes from a UUIRTLearnIR 
		/// are much larger and contain all the necessary data to reproduce a code, 
		/// whereas the codes passed to IREventStr are simpler representations of IR codes 
		/// only long enough to be unique).
		/// </remarks>
		/// <param name="hDrvHandle">Handle to to USB-UIRT returned by UUIRTOpen</param>
		/// <param name="receiveProc">the address of a 
		/// PUUCALLBACKPROC function</param>
		/// <param name="userData">a general-purpose 
		/// 32-bit value supplied by the caller to UUIRTSetReceiveCallback. This parameter 
		/// is useful for carrying context information, etc. Will be passed to receiveProc.</param>
		/// <returns>TRUE on success</returns>
		[DllImport("uuirtdrv.dll", SetLastError=true)]
		private static extern bool UUIRTSetReceiveCallback(
			IntPtr hDrvHandle, 
			ReceiveCallback receiveProc, 
			IntPtr userData);


		private const Int32 INVALID_HANDLE_VALUE = -1;

		private enum UuirtDriverError
		{
			/// <summary>
			/// Unable to connect to USB-UIRT device
			/// </summary>
			NoDeviceFound = 0x20000001,
			/// <summary>
			/// Unable to communicate with USB-UIRT device
			/// </summary>
			NoResponse = 0x20000002,
			/// <summary>
			/// Unable to find USB-UIRT Driver
			/// </summary>
			NoDll = 0x20000003,
			/// <summary>
			/// USB-UIRT's firmware is not compatible with this API DLL
			/// </summary>
			WrongVersion = 0x20000004
		}
		#endregion
	}

}

