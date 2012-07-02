using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bitcoin.Core {
	class TransactionInput {
		private NetworkParameters n;
		private Transaction t;
		private byte[] bytes;

		public TransactionInput(NetworkParameters n, Transaction t, byte[] bytes) {
			// TODO: Complete member initialization
			this.n = n;
			this.t = t;
			this.bytes = bytes;
		}
	}
}
