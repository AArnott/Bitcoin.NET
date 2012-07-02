namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Numerics;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// A Message is a data structure that can be serialized/deserialized using both the BitCoin proprietary serialization
	/// format and built-in Java object serialization. Specific types of messages that are used both in the block chain,
	/// and on the wire, are derived from this class.
	/// </summary>
	/// <remarks>
	/// <para>This class is not useful for library users. If you want to talk to the network see the {@link Peer} class.</para>
	/// </remarks>
	[Serializable]
	public abstract class Message {
		private const long serialVersionUID = -3561053461717079135L;

		public const int MAX_SIZE = 0x02000000;
		public const int UNKNOWN_LENGTH = -1;

		// Useful to ensure serialize/deserialize are consistent with each other.
		private const bool SELF_CHECK = false;

		// The offset is how many bytes into the provided byte array this message starts at.
		protected int offset;
		// The cursor keeps track of where we are in the byte array as we parse it.
		// Note that it's relative to the start of the array NOT the start of the message.
		protected int cursor;

		protected int Length = UNKNOWN_LENGTH;

		// The raw message bytes themselves.
		protected byte[] bytes;

		protected bool parsed = false;
		protected bool recached = false;
		protected readonly bool parseLazy;
		protected readonly bool parseRetain;

		protected int protocolVersion;

		protected byte[] checksum;

		// This will be saved by subclasses that implement Serializable.
		protected NetworkParameters networkParameters;

		/**
		 * This exists for the Java serialization framework to use only.
		 */
		protected Message() {
			this.parsed = true;
			this.parseLazy = false;
			this.parseRetain = false;
		}

		Message(NetworkParameters networkParameters) {
			this.networkParameters = networkParameters;
			parsed = true;
			parseLazy = false;
			parseRetain = false;
		}

		Message(NetworkParameters networkParameters, byte[] msg, int offset, int protocolVersion)
			: this(networkParameters, msg, offset, protocolVersion, false, false, UNKNOWN_LENGTH) {
		}

		/**
		 * 
		 * @param networkParameters NetworkParameters object.
		 * @param msg Bitcoin protocol formatted byte array containing message content.
		 * @param offset The location of the first msg byte within the array.
		 * @param protocolVersion Bitcoin protocol version.
		 * @param parseLazy Whether to perform a full parse immediately or delay until a read is requested.
		 * @param parseRetain Whether to retain the backing byte array for quick reserialization.  
		 * If true and the backing byte array is invalidated due to modification of a field then 
		 * the cached bytes may be repopulated and retained if the message is serialized again in the future.
		 * @param Length The Length of message if known.  Usually this is provided when deserializing of the wire
		 * as the Length will be provided as part of the header.  If unknown then set to Message.UNKNOWN_LENGTH
		 * @
		 */
		Message(NetworkParameters networkParameters, byte[] msg, int offset, int protocolVersion, bool parseLazy, bool parseRetain, int Length) {
			this.parseLazy = parseLazy;
			this.parseRetain = parseRetain;
			this.protocolVersion = protocolVersion;
			this.networkParameters = networkParameters;
			this.bytes = msg;
			this.cursor = this.offset = offset;
			this.Length = Length;
			if (parseLazy) {
				parseLite();
			} else {
				parseLite();
				parse();
				parsed = true;
			}

			Assumes.True(this.Length != UNKNOWN_LENGTH,
					"Length field has not been set in constructor for %s after %s parse. " +
							"Refer to Message.parseLite() for detail of required Length field contract.",
					this.GetType().Name, parseLazy ? "lite" : "full");

			if (SELF_CHECK) {
				selfCheck(msg, offset);
			}

			if (parseRetain || !parsed)
				return;
			this.bytes = null;
		}

		private void selfCheck(byte[] msg, int offset) {
			if (!(this is VersionMessage)) {
				maybeParse();
				byte[] msgbytes = new byte[cursor - offset];
				msg.CopyTo(offset, msgbytes, 0, cursor - offset);
				byte[] reserialized = bitcoinSerialize();
				if (!Utils.ArraysEqual(reserialized, msgbytes))
					throw new ApplicationException("Serialization is wrong: \n" +
							Hex.Encode(reserialized) + " vs \n" +
							Hex.Encode(msgbytes));
			}
		}

		Message(NetworkParameters networkParameters, byte[] msg, int offset)
			: this(networkParameters, msg, offset, NetworkParameters.PROTOCOL_VERSION, false, false, UNKNOWN_LENGTH) {
		}

		Message(NetworkParameters networkParameters, byte[] msg, int offset, bool parseLazy, bool parseRetain, int Length) :
			this(networkParameters, msg, offset, NetworkParameters.PROTOCOL_VERSION, parseLazy, parseRetain, Length) {
		}

		// These methods handle the serialization/deserialization using the custom BitCoin protocol.
		// It's somewhat painful to work with in Java, so some of these objects support a second 
		// serialization mechanism - the standard Java serialization system. This is used when things 
		// are serialized to the wallet.
		protected abstract void parse();

		/**
		 * Perform the most minimal parse possible to calculate the Length of the message.
		 * This is only required for subclasses of ChildClass as root level messages will have their Length passed
		 * into the constructor.
		 * <p/>
		 * Implementations should adhere to the following contract:  If parseLazy = true the 'Length'
		 * field must be set before returning.  If parseLazy = false the Length field must be set either
		 * within the parseLite() method OR the parse() method.  The overriding requirement is that Length
		 * must be set to non UNKNOWN_MESSAGE value by the time the constructor exits.
		 *
		 * @return
		 * @
		 */
		protected abstract void parseLite();

		/**
		 * Ensure the object is parsed if needed.  This should be called in every getter before returning a value.
		 * If the lazy parse flag is not set this is a method returns immediately.
		 */
		protected void maybeParse() {
			if (parsed || bytes == null)
				return;
			try {
				parse();
				parsed = true;
				if (!parseRetain)
					bytes = null;
			} catch (ProtocolException e) {
				throw new LazyParseException("ProtocolException caught during lazy parse.  For safe access to fields call ensureParsed before attempting read or write access", e);
			}
		}

		/**
		 * In lazy parsing mode access to getters and setters may throw an unchecked LazyParseException.  If guaranteed safe access is required
		 * this method will force parsing to occur immediately thus ensuring LazyParseExeption will never be thrown from this Message.
		 * If the Message contains child messages (e.g. a Block containing Transaction messages) this will not force child messages to parse.
		 * <p/>
		 * This could be overidden for Transaction and it's child classes to ensure the entire tree of Message objects is parsed.
		 *
		 * @
		 */
		public void ensureParsed() {
			try {
				maybeParse();
			} catch (LazyParseException e) {
				if (e.InnerException is ProtocolException)
					throw e.InnerException;
				throw new ProtocolException(e);
			}
		}

		/**
		 * To be called before any change of internal values including any setters.  This ensures any cached byte array is
		 * removed after performing a lazy parse if necessary to ensure the object is fully populated.
		 * <p/>
		 * Child messages of this object(e.g. Transactions belonging to a Block) will not have their internal byte caches
		 * invalidated unless they are also modified internally.
		 */
		protected void unCache() {
			maybeParse();
			checksum = null;
			bytes = null;
			recached = false;
		}

		protected void adjustLength(int adjustment) {
			if (Length != UNKNOWN_LENGTH)
				// Our own Length is now unknown if we have an unknown Length adjustment.
				Length = adjustment == UNKNOWN_LENGTH ? UNKNOWN_LENGTH : Length + adjustment;
		}

		/**
		 * used for unit testing
		 */
		public bool isParsed() {
			return parsed;
		}

		/**
		 * used for unit testing
		 */
		public bool isCached() {
			//return parseLazy ? parsed && bytes != null : bytes != null;
			return bytes != null;
		}

		public bool isRecached() {
			return recached;
		}

		/**
		 * Should only used by BitcoinSerializer for cached checksum
		 *
		 * @return the checksum
		 */
		byte[] getChecksum() {
			return checksum;
		}

		/**
		 * Should only used by BitcoinSerializer for caching checksum
		 *
		 * @param checksum the checksum to set
		 */
		void setChecksum(byte[] checksum) {
			Requires.True(checksum.Length == 4, "Checksum Length must be 4 bytes, actual Length: " + checksum.Length);
			this.checksum = checksum;
		}

		/**
		 * Returns a copy of the array returned by {@link Message#unsafeBitcoinSerialize()}, which is safe to mutate.
		 * If you need extra performance and can guarantee you won't write to the array, you can use the unsafe version.
		 *
		 * @return a freshly allocated serialized byte array
		 */
		public byte[] bitcoinSerialize() {
			byte[] bytes = unsafeBitcoinSerialize();
			byte[] copy = new byte[bytes.Length];
			bytes.CopyTo(0, copy, 0, bytes.Length);
			return copy;
		}

		/**
		 * Serialize this message to a byte array that conforms to the bitcoin wire protocol.
		 * <br/>
		 * This method may return the original byte array used to construct this message if the
		 * following conditions are met:
		 * <ol>
		 * <li>1) The message was parsed from a byte array with parseRetain = true</li>
		 * <li>2) The message has not been modified</li>
		 * <li>3) The array had an offset of 0 and no surplus bytes</li>
		 * </ol>
		 *
		 * If condition 3 is not met then an copy of the relevant portion of the array will be returned.
		 * Otherwise a full serialize will occur. For this reason you should only use this API if you can guarantee you
		 * will treat the resulting array as read only.
		 *
		 * @return a byte array owned by this object, do NOT mutate it.
		 */
		public byte[] unsafeBitcoinSerialize() {
			// 1st attempt to use a cached array.
			if (bytes != null) {
				if (offset == 0 && Length == bytes.Length) {
					// Cached byte array is the entire message with no extras so we can return as is and avoid an array
					// copy.
					return bytes;
				}

				byte[] buf = new byte[Length];
				bytes.CopyTo(offset, buf, 0, Length);
				return buf;
			}

			// No cached array available so serialize parts by stream.
			var stream = new MemoryStream(Length < 32 ? 32 : Length + 32);
			var writer = new BinaryWriter(stream);
			bitcoinSerializeToStream(writer);
			writer.Flush();

			if (parseRetain) {
				// A free set of steak knives!
				// If there happens to be a call to this method we gain an opportunity to recache
				// the byte array and in this case it contains no bytes from parent messages.
				// This give a dual benefit.  Releasing references to the larger byte array so that it
				// it is more likely to be GC'd.  And preventing double serializations.  E.g. calculating
				// merkle root calls this method.  It is will frequently happen prior to serializing the block
				// which means another call to bitcoinSerialize is coming.  If we didn't recache then internal
				// serialization would occur a 2nd time and every subsequent time the message is serialized.
				bytes = stream.ToArray();
				cursor = cursor - offset;
				offset = 0;
				recached = true;
				Length = bytes.Length;
				return bytes;
			}

			// Record Length. If this Message wasn't parsed from a byte stream it won't have Length field
			// set (except for static Length message types).  Setting it makes future streaming more efficient
			// because we can preallocate the ByteArrayOutputStream buffer and avoid resizing.
			byte[] buf2 = stream.ToArray();
			Length = buf2.Length;
			return buf2;
		}

		/**
		 * Serialize this message to the provided OutputStream using the bitcoin wire format.
		 *
		 * @param stream
		 * @
		 */
		public void bitcoinSerialize(BinaryWriter stream) {
			// 1st check for cached bytes.
			if (bytes != null && Length != UNKNOWN_LENGTH) {
				stream.Write(bytes, offset, Length);
				return;
			}

			bitcoinSerializeToStream(stream);
		}

		/**
		 * Serializes this message to the provided stream. If you just want the raw bytes use bitcoinSerialize().
		 */
		void bitcoinSerializeToStream(BinaryWriter stream) {
			Logger.Log.DebugFormat("Warning: {} class has not implemented bitcoinSerializeToStream method.  Generating message with no payload", this.GetType().Name);
		}

		/**
		 * This method is a NOP for all classes except Block and Transaction.  It is only declared in Message
		 * so BitcoinSerializer can avoid 2 is checks + a casting.
		 *
		 * @return
		 */
		public Sha256Hash getHash() {
			return null;
		}

		/**
		 * This should be overridden to extract correct message size in the case of lazy parsing.  Until this method is
		 * implemented in a subclass of ChildMessage lazy parsing may have no effect.
		 *
		 * This default implementation is a safe fall back that will ensure it returns a correct value by parsing the message.
		 *
		 * @return
		 */
		int getMessageSize() {
			if (Length != UNKNOWN_LENGTH)
				return Length;
			maybeParse();
			Assumes.True(Length != UNKNOWN_LENGTH,
					"Length field has not been set in %s after full parse.", this.GetType().Name);
			return Length;
		}

		long readUint32() {
			long u = Utils.readUint32(bytes, cursor);
			cursor += 4;
			return u;
		}

		Sha256Hash readHash() {
			byte[] hash = new byte[32];
			bytes.CopyTo(cursor, hash, 0, 32);
			// We have to flip it around, as it's been read off the wire in little endian.
			// Not the most efficient way to do this but the clearest.
			hash = Utils.ReverseOrder(hash);
			cursor += 32;
			return new Sha256Hash(hash);
		}


		BigInteger readUint64() {
			// Java does not have an unsigned 64 bit type. So scrape it off the wire then flip.
			byte[] valbytes = new byte[8];
			bytes.CopyTo(cursor, valbytes, 0, 8);
			valbytes = Utils.ReverseOrder(valbytes);
			cursor += valbytes.Length;
			return new BigInteger(valbytes);
		}

		long readVarInt() {
			return readVarInt(0);
		}

		long readVarInt(int offset) {
			VarInt varint = new VarInt(bytes, cursor + offset);
			cursor += offset + varint.getSizeInBytes();
			return varint.value;
		}


		byte[] readBytes(int Length) {
			byte[] b = new byte[Length];
			bytes.CopyTo(cursor, b, 0, Length);
			cursor += Length;
			return b;
		}

		byte[] readByteArray() {
			long len = readVarInt();
			return readBytes((int)len);
		}

		String readStr() {
			VarInt varInt = new VarInt(bytes, cursor);
			if (varInt.value == 0) {
				cursor += 1;
				return "";
			}
			cursor += varInt.getSizeInBytes();
			byte[] characters = new byte[(int)varInt.value];
			bytes.CopyTo(cursor, characters, 0, characters.Length);
			cursor += characters.Length;
			return Encoding.UTF8.GetString(characters);
		}

		public class LazyParseException : Exception {
			private const long serialVersionUID = 6971943053112975594L;

			public LazyParseException(String message, Exception innerException) :
				base(message, innerException) {
			}

			public LazyParseException(String message)
				: base(message) {
			}

		}
	}
}
