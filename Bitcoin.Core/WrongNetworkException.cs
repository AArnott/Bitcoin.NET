namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class WrongNetworkException : Exception {
		/** The version code that was provided in the address. */
		private readonly int verCode;
		/** The list of acceptable versions that were expected given the addresses network parameters. */
		private readonly IReadOnlyList<int> acceptableVersions;

		/// <summary>
		/// Initializes a new instance of the <see cref="WrongNetworkException"/> class.
		/// </summary>
		public WrongNetworkException(int verCode, IReadOnlyList<int> acceptableVersions)
			: base("Version code of address did not match acceptable versions for network: " + verCode + " not in " +
				  Utils.ToString(acceptableVersions)) {
			this.verCode = verCode;
			this.acceptableVersions = acceptableVersions;
		}

		public int VerCode {
			get { return this.verCode; }
		}

		public IReadOnlyList<int> AcceptableVersions {
			get { return this.acceptableVersions; }
		}
	}
}
