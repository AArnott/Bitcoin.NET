namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal static class Hex {
		internal static byte[] Decode(string hexEncoding) {
			byte[] bytes = new byte[hexEncoding.Length / 2];
			for (int i = 0; i < hexEncoding.Length; i += 2) {
				bytes[i / 2] = Convert.ToByte(hexEncoding.Substring(i, 2), 16);
			}

			return bytes;
		}

		internal static string Encode(byte[] buffer) {
			var hex = new StringBuilder(buffer.Length * 2);
			foreach (byte b in buffer) {
				hex.AppendFormat("{0:x2}", b);
			}

			return hex.ToString();
		}
	}
}
