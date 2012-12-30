using System;

namespace UsbUirt
{
	/// <summary>
	/// Defines special modifiers used when learning IR codes.
	/// </summary>
	public enum LearnCodeModifier 
	{
		/// <summary>
		/// Use default learning.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// Force RAW mode.
		/// </summary>
		ForceRaw = 0x0100,
		/// <summary>
		/// Force Struct-mode.
		/// </summary>
		ForceStruct = 0x0200,
		/// <summary>
		/// Force a particular frequency.
		/// </summary>
		/// <remarks>When (and only when) using ForceFrequency, you must call the appropriate
		/// overload of Learn() or LearnAsync() and provide the expected frequency value.</remarks>
		ForceFrequency = 0x0400,
		/// <summary>
		/// Automatically detect the frequency.
		/// </summary>
		FrequencyDetect = 0x0800
	}
}
