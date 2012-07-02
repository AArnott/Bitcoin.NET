namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class Transaction {
		private NetworkParameters n;

		public Transaction(NetworkParameters n) {
			// TODO: Complete member initialization
			this.n = n;
		}

		internal void addOutput(TransactionOutput transactionOutput) {
			throw new NotImplementedException();
		}

		internal void addInput(TransactionInput transactionInput) {
			throw new NotImplementedException();
		}
	}
}
