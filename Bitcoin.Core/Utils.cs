namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Numerics;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;

	public static class Utils {
		/// <summary>
		/// Calculates the SHA-256 hash of the given byte range, and then hashes the resulting hash again. This is
		/// standard procedure in BitCoin. The resulting hash is in big endian form.
		/// </summary>
		internal static byte[] DoubleDigest(byte[] input) {
			Requires.NotNull(input, "input");
			return DoubleDigest(input, 0, input.Length);
		}

		/// <summary>
		/// Calculates the SHA-256 hash of the given byte range, and then hashes the resulting hash again. This is
		/// standard procedure in BitCoin. The resulting hash is in big endian form.
		/// </summary>
		internal static byte[] DoubleDigest(byte[] input, int offset, int count) {
			Requires.NotNull(input, "input");
			Requires.InRange(offset < input.Length, "offset");
			Requires.InRange(offset + count <= input.Length, "count");

			using (var hashAlgorithm = HashAlgorithm.Create("SHA-256")) {
				byte[] hash = hashAlgorithm.ComputeHash(input, offset, count);
				return hashAlgorithm.ComputeHash(hash);
			}
		}

		internal static bool ArraysEqual(IReadOnlyList<byte> array1, IReadOnlyList<byte> array2) {
			Requires.NotNull(array1, "array1");
			Requires.NotNull(array2, "array2");

			if (array1.Count != array2.Count) {
				return false;
			}

			for (int i = 0; i < array1.Count; i++) {
				if (array1[i] != array2[i]) {
					return false;
				}
			}

			return true;
		}

		internal static int GetArrayHashCode(IReadOnlyList<byte> array) {
			Requires.NotNull(array, "array");

			// TODO: code here
			return 0;
		}

		internal static void CopyTo(this IReadOnlyList<byte> sourceBuffer, int sourceIndex, byte[] destinationBuffer, int destinationIndex, int length) {
			Requires.NotNull(sourceBuffer, "sourceBuffer");
			Requires.InRange(sourceIndex >= 0 && sourceIndex < sourceBuffer.Count, "sourceIndex");
			Requires.NotNull(destinationBuffer, "destinationBuffer");
			Requires.InRange(destinationIndex >= 0 && destinationIndex < destinationBuffer.Length, "destinationIndex");
			Requires.InRange(destinationIndex + length <= destinationBuffer.Length, "length");
			Requires.InRange(sourceIndex + length <= sourceBuffer.Count, "length");

			while (length-- > 0) {
				destinationBuffer[destinationIndex++] = sourceBuffer[sourceIndex++];
			}
		}

		internal static byte[] MutableCopy(this IReadOnlyList<byte> readOnlyBuffer) {
			Requires.NotNull(readOnlyBuffer, "readOnlyBuffer");

			var buffer = new byte[readOnlyBuffer.Count];
			for (int i = 0; i < readOnlyBuffer.Count; i++) {
				buffer[i] = readOnlyBuffer[i];
			}

			return buffer;
		}

		public static byte[] ReverseOrder(this IReadOnlyList<byte> input) {
			Requires.NotNull(input, "input");

			var buffer = new byte[input.Count];
			for (int i = 0; i < input.Count; i++) {
				buffer[input.Count - i - 1] = input[i];
			}

			return buffer;
		}

		// The representation of nBits uses another home-brew encoding, as a way to represent a large
		// hash value in only 32 bits.
		internal static BigInteger decodeCompactBits(long compact) {
			int size = ((int)(compact >> 24)) & 0xFF;
			byte[] bytes = new byte[4 + size];
			bytes[3] = (byte)size;
			if (size >= 1) bytes[4] = (byte)((compact >> 16) & 0xFF);
			if (size >= 2) bytes[5] = (byte)((compact >> 8) & 0xFF);
			if (size >= 3) bytes[6] = (byte)((compact >> 0) & 0xFF);
			return decodeMPI(bytes);
		}

		/**
		* MPI encoded numbers are produced by the OpenSSL BN_bn2mpi function. They consist of
		* a 4 byte big endian length field, followed by the stated number of bytes representing
		* the number in big endian format.
		*/
		private static BigInteger decodeMPI(byte[] mpi) {
			int length = (int)readUint32BE(mpi, 0);
			byte[] buf = new byte[length];
			mpi.CopyTo(4, buf, 0, length);
			return new BigInteger(buf);
		}

		/**
 * Work around lack of unsigned types in Java.
 */
		internal static bool isLessThanUnsigned(long n1, long n2) {
			return (n1 < n2) ^ ((n1 < 0) != (n2 < 0));
		}


		public static void uint32ToByteArrayLE(long val, byte[] output, int offset) {
			output[offset + 0] = (byte)(0xFF & (val >> 0));
			output[offset + 1] = (byte)(0xFF & (val >> 8));
			output[offset + 2] = (byte)(0xFF & (val >> 16));
			output[offset + 3] = (byte)(0xFF & (val >> 24));
		}

		internal static long readUint32(byte[] bytes, int offset) {
			return ((bytes[offset++] & 0xFFL) << 0) |
					((bytes[offset++] & 0xFFL) << 8) |
					((bytes[offset++] & 0xFFL) << 16) |
					((bytes[offset] & 0xFFL) << 24);
		}

		private static long readUint32BE(byte[] bytes, int offset) {
			return ((bytes[offset + 0] & 0xFFL) << 24) |
					((bytes[offset + 1] & 0xFFL) << 16) |
					((bytes[offset + 2] & 0xFFL) << 8) |
					((bytes[offset + 3] & 0xFFL) << 0);
		}

		internal static string ToString<T>(IReadOnlyList<T> list) {
			Requires.NotNull(list, "list");
			var builder = new StringBuilder();
			builder.Append("{");

			foreach (var item in list) {
				if (builder.Length > 1) {
					builder.Append(",");
				}

				builder.Append(item);
			}

			builder.Append("}");
			return builder.ToString();
		}
	}
}
