namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	public class Address : VersionedChecksummedBytes {
		/**
		* An address is a RIPEMD160 hash of a public key, therefore is always 160 bits or 20 bytes.
		*/
		public const int LENGTH = 20;

		/**
		 * Construct an address from parameters and the hash160 form. Example:<p>
		 *
		 * <pre>new Address(NetworkParameters.prodNet(), Hex.decode("4a22c3c4cbb31e4d03b15550636762bda0baf85a"));</pre>
		 */
		public Address(NetworkParameters networkParameters, byte[] hash160)
			: base(networkParameters.addressHeader, hash160) {
			if (hash160.Length != 20) { // 160 = 8 * 20
				throw new ArgumentException("Addresses are 160-bit hashes, so you must provide 20 bytes");
			}
		}

		/**
		 * Construct an address from parameters and the standard "human readable" form. Example:<p>
		 *
		 * <pre>new Address(NetworkParameters.prodNet(), "17kzeh4N8g49GFvdDzSf8PjaPfyoD1MndL");</pre><p>
		 *
		 * @param networkParameters The expected NetworkParameters or null if you don't want validation.
		 * @param address The textual form of the address, such as "17kzeh4N8g49GFvdDzSf8PjaPfyoD1MndL"
		 * @throws AddressFormatException if the given address doesn't parse or the checksum is invalid
		 * @throws WrongNetworkException if the given address is valid but for a different chain (eg testnet vs prodnet)
		 */
		public Address(NetworkParameters networkParameters, string address)
			: base(address) {
			if (networkParameters != null) {
				bool found = false;
				foreach (int v in networkParameters.acceptableAddressCodes) {
					if (this.Version == v) {
						found = true;
						break;
					}
				}
				if (!found) {
					throw new WrongNetworkException(this.Version, networkParameters.acceptableAddressCodes);
				}
			}
		}

		/** The (big endian) 20 byte hash that is the core of a BitCoin address. */
		public IReadOnlyList<byte> getHash160() {
			return this.Bytes;
		}

		/**
		 * Examines the version byte of the address and attempts to find a matching NetworkParameters. If you aren't sure
		 * which network the address is intended for (eg, it was provided by a user), you can use this to decide if it is
		 * compatible with the current wallet. You should be able to handle a null response from this method. Note that the
		 * parameters returned is not necessarily the same as the one the Address was created with.
		 *
		 * @return a NetworkParameters representing the network the address is intended for, or null if unknown.
		 */
		public NetworkParameters getParameters() {
			// TODO: There should be a more generic way to get all supported networks.
			NetworkParameters[] networks =
					new NetworkParameters[] { NetworkParameters.testNet(), NetworkParameters.prodNet() };

			foreach (NetworkParameters networkParameters in networks) {
				NetworkParameters nested = networkParameters;
				if (networkParameters.acceptableAddressCodes == null) {
					// Old Java-serialized wallet. This code can eventually be deleted.
					if (networkParameters.getId().Equals(NetworkParameters.ID_PRODNET))
						nested = NetworkParameters.prodNet();
					else if (networkParameters.getId().Equals(NetworkParameters.ID_TESTNET))
						nested = NetworkParameters.testNet();
				}

				foreach (int code in nested.acceptableAddressCodes) {
					if (code == this.Version) {
						return nested;
					}
				}
			}
			return null;
		}

		/**
		 * Given an address, examines the version byte and attempts to find a matching NetworkParameters. If you aren't sure
		 * which network the address is intended for (eg, it was provided by a user), you can use this to decide if it is
		 * compatible with the current wallet. You should be able to handle a null response from this method.
		 *
		 * @param address
		 * @return a NetworkParameters representing the network the address is intended for, or null if unknown.
		 */
		public static NetworkParameters getParametersFromAddress(string address) {
			return new Address(null, address).getParameters();
		}
	}
}
