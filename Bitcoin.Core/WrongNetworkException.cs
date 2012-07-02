namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class WrongNetworkException : Exception {
		private int p1;
		private int[] p2;

		/// <summary>
		/// Initializes a new instance of the <see cref="WrongNetworkException"/> class.
		/// </summary>
		public WrongNetworkException(string message, Exception innerException = null)
			: base(message, innerException) {
		}

		public WrongNetworkException(int p1, int[] p2) {
			// TODO: Complete member initialization
			this.p1 = p1;
			this.p2 = p2;
		}
	}
}
