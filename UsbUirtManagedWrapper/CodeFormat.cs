using System;

namespace UsbUirt
{
	/// <summary>
	/// Defines the code formats used when transmitting or learning IR codes.
	/// </summary>
	public enum CodeFormat
	{
		/// <summary>
		/// Uuirt code format
		/// </summary>
		Uuirt = 0x0000,
		/// <summary>
		/// Pronto format
		/// </summary>
		Pronto = 0x0010
	}
}
