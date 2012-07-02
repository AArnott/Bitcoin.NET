namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// A custom form of base58 is used to encode BitCoin addresses. Note that this is not the same base58 as used by
	/// Flickr, which you may see reference to around the internet.
	/// </summary>
	/// <remarks>
	/// <p>Satoshi says: why base-58 instead of standard base-64 encoding?</p>
	/// <ul>
	/// <li>Don't want 0OIl characters that look the same in some fonts and
	///    could be used to create visually identical looking account numbers.</li>
	/// <li>A string with non-alphanumeric characters is not as easily accepted as an account number.</li>
	/// <li>E-mail usually won't line-break if there's no punctuation to break at.</li>
	/// <li>Doubleclicking selects the whole number as one word if it's all alphanumeric.</li>
	/// </ul>
	/// </remarks>
	public static class Base58 {
		private static readonly char[] Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz".ToCharArray();
		private static readonly int Base58Length = Alphabet.Length;
		private static readonly int[] INDEXES = new int[128];
		private const int Base256Length = 256;

		static Base58() {
			for (int i = 0; i < INDEXES.Length; i++) {
				INDEXES[i] = -1;
			}

			for (int i = 0; i < Alphabet.Length; i++) {
				INDEXES[Alphabet[i]] = i;
			}
		}

		/// <summary>
		/// Encodes an arbitrary buffer into a base58 encoded string.
		/// </summary>
		public static string Encode(IReadOnlyList<byte> input) {
			Requires.NotNull(input, "input");

			var buffer = input.MutableCopy();

			// The actual encoding.
			char[] temp = new char[buffer.Length * 2];
			int j = temp.Length;

			int zeroCount = LeadingZerosCount(buffer);
			int startAt = zeroCount;
			while (startAt < buffer.Length) {
				byte mod = divmod58(buffer, startAt);
				if (buffer[startAt] == 0) {
					++startAt;
				}
				temp[--j] = Alphabet[mod];
			}

			// Strip extra '1' if there are some after decoding.
			while (j < temp.Length && temp[j] == Alphabet[0]) {
				++j;
			}

			// Add as many leading '1' as there were leading zeros.
			while (--zeroCount >= 0) {
				temp[--j] = Alphabet[0];
			}

			return new string(temp, j, temp.Length - j);
		}

		/// <summary>
		/// Decodes a base58 encoded string into its original buffer.
		/// </summary>
		public static byte[] Decode(string input) {
			Requires.NotNull(input, "input");

			if (input.Length == 0) {
				return new byte[0];
			}

			byte[] input58 = new byte[input.Length];

			// Transform the String to a base58 byte sequence
			for (int i = 0; i < input.Length; ++i) {
				char c = input[i];

				int digit58 = -1;
				if (c >= 0 && c < 128) {
					digit58 = INDEXES[c];
				}

				if (digit58 < 0) {
					throw new AddressFormatException("Illegal character " + c + " at " + i);
				}

				input58[i] = (byte)digit58;
			}

			// Count leading zeroes
			int zeroCount = 0;
			while (zeroCount < input58.Length && input58[zeroCount] == 0) {
				++zeroCount;
			}

			// The encoding
			byte[] temp = new byte[input.Length];
			int j = temp.Length;

			int startAt = zeroCount;
			while (startAt < input58.Length) {
				byte mod = divmod256(input58, startAt);
				if (input58[startAt] == 0) {
					++startAt;
				}

				temp[--j] = mod;
			}

			// Do no add extra leading zeroes, move j to first non null byte.
			while (j < temp.Length && temp[j] == 0) {
				++j;
			}

			var result = new byte[temp.Length - (j - zeroCount)];
			Array.Copy(temp, j - zeroCount, result, 0, result.Length);
			return result;
		}

		/// <summary>
		/// Uses the checksum in the last 4 bytes of the decoded data to verify the rest are correct. The checksum is
		/// removed from the returned data.
		/// </summary>
		/// <exception cref="AddressFormatException">if the input is not base 58 or the checksum does not validate.</exception>
		public static byte[] DecodeChecked(string input) {
			byte[] tmp = Decode(input);
			if (tmp.Length < 4) {
				throw new AddressFormatException("Input to short");
			}

			byte[] bytes = copyOfRange(tmp, 0, tmp.Length - 4);
			byte[] checksum = copyOfRange(tmp, tmp.Length - 4, tmp.Length);

			tmp = Utils.DoubleDigest(bytes);
			byte[] hash = copyOfRange(tmp, 0, 4);
			if (!Utils.ArraysEqual(checksum, hash)) {
				throw new AddressFormatException("Checksum does not validate");
			}

			return bytes;
		}

		private static int LeadingZerosCount(IReadOnlyList<byte> buffer) {
			Requires.NotNull(buffer, "buffer");

			int leadingZeros = 0;
			for (leadingZeros = 0; leadingZeros < buffer.Count && buffer[leadingZeros] == 0; leadingZeros++) ;
			return leadingZeros;
		}

		/// <summary>
		/// number -> number / 58, returns number % 58
		/// </summary>
		private static byte divmod58(byte[] number, int startAt) {
			Requires.NotNull(number, "number");

			int remainder = 0;
			for (int i = startAt; i < number.Length; i++) {
				int digit256 = (int)number[i] & 0xFF;
				int temp = remainder * Base256Length + digit256;

				number[i] = (byte)(temp / Base58Length);

				remainder = temp % Base58Length;
			}

			return (byte)remainder;
		}

		/// <summary>
		/// number -> number / 256, returns number % 256
		/// </summary>
		private static byte divmod256(byte[] number58, int startAt) {
			Requires.NotNull(number58, "number58");

			int remainder = 0;
			for (int i = startAt; i < number58.Length; i++) {
				int digit58 = (int)number58[i] & 0xFF;
				int temp = remainder * Base58Length + digit58;

				number58[i] = (byte)(temp / Base256Length);

				remainder = temp % Base256Length;
			}

			return (byte)remainder;
		}

		private static byte[] copyOfRange(byte[] buffer, int start, int end) {
			var result = new byte[end - start];
			Array.Copy(buffer, start, result, 0, end - start);
			return result;
		}
	}
}
