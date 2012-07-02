namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	public class ProtocolException : Exception {
		public ProtocolException(Exception innerException)
			: base(String.Empty, innerException) {
		}

		public ProtocolException(string message, Exception innerException)
			: base(message, innerException) {
		}
	}
}
