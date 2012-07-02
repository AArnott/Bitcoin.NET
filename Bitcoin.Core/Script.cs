namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	internal class Script {
  // Some constants used for decoding the scripts.
    public const int OP_PUSHDATA1 = 76;
    public const int OP_PUSHDATA2 = 77;
    public const int OP_PUSHDATA4 = 78;
    public const int OP_DUP = 118;
    public const int OP_HASH160 = 169;
    public const int OP_EQUALVERIFY = 136;
    public const int OP_CHECKSIG = 172;

	    ////////////////////// Interface for writing scripts from scratch ////////////////////////////////

    /**
     * Writes out the given byte buffer to the output stream with the correct opcode prefix
     */
    internal static void writeBytes(BinaryWriter os, byte[] buf) {
        if (buf.Length < OP_PUSHDATA1) {
            os.Write(buf.Length);
            os.Write(buf);
        } else if (buf.Length < 256) {
            os.Write(OP_PUSHDATA1);
            os.Write(buf.Length);
            os.Write(buf);
        } else if (buf.Length < 65536) {
            os.Write(OP_PUSHDATA2);
            os.Write(0xFF & (buf.Length));
            os.Write(0xFF & (buf.Length >> 8));
            os.Write(buf);
        } else {
            Requires.Fail("buf", "Unsupported buffer size");
        }
    }
}
}
