namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// An exception thrown when a base58 decoding fails.
	/// </summary>
	public class AddressFormatException : Exception {
		/// <summary>
		/// Initializes a new instance of the <see cref="AddressFormatException"/> class.
		/// </summary>
		public AddressFormatException(string message, Exception innerException = null)
			: base(message, innerException) {
		}
	}
}
