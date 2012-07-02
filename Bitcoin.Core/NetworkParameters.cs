﻿namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Numerics;
	using System.Text;
	using System.Threading.Tasks;

	[Serializable]
	public class NetworkParameters {
		private const long serialVersionUID = 3L;

		/**
		 * The protocol version this library implements. A value of 31800 means 0.3.18.00.
		 */
		public const int PROTOCOL_VERSION = 31800;

		/**
		 * The alert signing key originally owned by Satoshi, and now passed on to Gavin along with a few others.
		 */
		public static readonly byte[] SATOSHI_KEY = Hex.Decode("04fc9702847840aaf195de8442ebecedf5b095cdbb9bc716bda9110971b28a49e0ead8564ff0db22209e0374782c093bb899692d524e9d6a6956e7c5ecbcd68284");

		/**
		 * The string returned by getId() for the main, production network where people trade things.
		 */
		public const string ID_PRODNET = "org.bitcoin.production";
		/**
		 * The string returned by getId() for the testnet.
		 */
		public const string ID_TESTNET = "org.bitcoin.test";


		// TODO: Seed nodes and checkpoint values should be here as well.

		/**
		 * Genesis block for this chain.<p>
		 *
		 * The first block in every chain is a well known constant shared between all BitCoin implemenetations. For a
		 * block to be valid, it must be eventually possible to work backwards to the genesis block by following the
		 * prevBlockHash pointers in the block headers.<p>
		 *
		 * The genesis blocks for both test and prod networks contain the timestamp of when they were created,
		 * and a message in the coinbase transaction. It says, <i>"The Times 03/Jan/2009 Chancellor on brink of second
		 * bailout for banks"</i>.
		 */
		public Block genesisBlock;
		/** What the easiest allowable proof of work should be. */
		public BigInteger proofOfWorkLimit;
		/** Default TCP port on which to connect to nodes. */
		public int port;
		/** The header bytes that identify the start of a packet on this network. */
		public long packetMagic;
		/**
		 * First byte of a base58 encoded address. See {@link Address}. This is the same as acceptableAddressCodes[0] and
		 * is the one used for "normal" addresses. Other types of address may be encountered with version codes found in
		 * the acceptableAddressCodes array.
		 */
		public int addressHeader;
		/** First byte of a base58 encoded dumped private key. See {@link DumpedPrivateKey}. */
		public int dumpedPrivateKeyHeader;
		/** How many blocks pass between difficulty adjustment periods. BitCoin standardises this to be 2015. */
		public int interval;
		/**
		 * How much time in seconds is supposed to pass between "interval" blocks. If the actual elapsed time is
		 * significantly different from this value, the network difficulty formula will produce a different value. Both
		 * test and production Bitcoin networks use 2 weeks (1209600 seconds).
		 */
		public int targetTimespan;
		/**
		 * The key used to sign {@link AlertMessage}s. You can use {@link ECKey#verify(byte[], byte[], byte[])} to verify
		 * signatures using it.
		 */
		public byte[] alertSigningKey;

		/**
		 * See getId(). This may be null for old deserialized wallets. In that case we derive it heuristically
		 * by looking at the port number.
		 */
		private String id;

		/**
		 * The version codes that prefix addresses which are acceptable on this network. Although Satoshi intended these to
		 * be used for "versioning", in fact they are today used to discriminate what kind of data is contained in the
		 * address and to prevent accidentally sending coins across chains which would destroy them.
		 */
		public int[] acceptableAddressCodes;

		private static Block createGenesis(NetworkParameters n) {
			Block genesisBlock = new Block(n);
			Transaction t = new Transaction(n);
			
			// A script containing the difficulty bits and the following message:
			//
			//   "The Times 03/Jan/2009 Chancellor on brink of second bailout for banks"
			byte[] bytes = Hex.Decode
					("04ffff001d0104455468652054696d65732030332f4a616e2f32303039204368616e63656c6c6f72206f6e206272696e6b206f66207365636f6e64206261696c6f757420666f722062616e6b73");
			t.addInput(new TransactionInput(n, t, bytes));

			var stream = new MemoryStream();
			var scriptPubKeyBytes = new BinaryWriter(stream);
			Script.writeBytes(scriptPubKeyBytes, Hex.Decode
					("04678afdb0fe5548271967f1a67130b7105cd6a828e03909a67962e0ea1f61deb649f6bc3f4cef38c4f35504e51ec112de5c384df7ba0b8d578a4c702b6bf11d5f"));
			scriptPubKeyBytes.Write(Script.OP_CHECKSIG);
			scriptPubKeyBytes.Flush();
			t.addOutput(new TransactionOutput(n, t, stream.ToArray()));

			genesisBlock.addTransaction(t);
			return genesisBlock;
		}

		public const int TARGET_TIMESPAN = 14 * 24 * 60 * 60;  // 2 weeks per difficulty cycle, on average.
		public const int TARGET_SPACING = 10 * 60;  // 10 minutes per block.
		public const int INTERVAL = TARGET_TIMESPAN / TARGET_SPACING;

		/** Sets up the given NetworkParameters with testnet values. */
		private static NetworkParameters createTestNet(NetworkParameters n) {
			// Genesis hash is 0000000224b1593e3ff16a0e3b61285bbc393a39f78c8aa48c456142671f7110
			n.proofOfWorkLimit = Utils.decodeCompactBits(0x1d0fffffL);
			n.packetMagic = 0xfabfb5daL;
			n.port = 18333;
			n.addressHeader = 111;
			n.acceptableAddressCodes = new int[] { 111 };
			n.dumpedPrivateKeyHeader = 239;
			n.interval = INTERVAL;
			n.targetTimespan = TARGET_TIMESPAN;
			n.alertSigningKey = SATOSHI_KEY;
			n.genesisBlock = createGenesis(n);
			n.genesisBlock.setTime(1296688602L);
			n.genesisBlock.setDifficultyTarget(0x1d07fff8L);
			n.genesisBlock.setNonce(384568319);
			n.id = ID_TESTNET;
			String genesisHash = n.genesisBlock.getHashAsString();
			Assumes.True(String.Equals(genesisHash, "00000007199508e34a9ff81e6ec0c477a4cccff2a4767a8eee39c11db367b008", StringComparison.OrdinalIgnoreCase));
			return n;
		}

		/** The test chain created by Gavin. */
		public static NetworkParameters testNet() {
			NetworkParameters n = new NetworkParameters();
			return createTestNet(n);
		}

		/** The primary BitCoin chain created by Satoshi. */
		public static NetworkParameters prodNet() {
			NetworkParameters n = new NetworkParameters();
			n.proofOfWorkLimit = Utils.decodeCompactBits(0x1d00ffffL);
			n.port = 8333;
			n.packetMagic = 0xf9beb4d9L;
			n.addressHeader = 0;
			n.acceptableAddressCodes = new int[] { 0 };
			n.dumpedPrivateKeyHeader = 128;
			n.interval = INTERVAL;
			n.targetTimespan = TARGET_TIMESPAN;
			n.alertSigningKey = SATOSHI_KEY;
			n.genesisBlock = createGenesis(n);
			n.genesisBlock.setDifficultyTarget(0x1d00ffffL);
			n.genesisBlock.setTime(1231006505L);
			n.genesisBlock.setNonce(2083236893);
			n.id = ID_PRODNET;
			String genesisHash = n.genesisBlock.getHashAsString();
			Assumes.True(String.Equals(genesisHash, "000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f", StringComparison.OrdinalIgnoreCase));
			return n;
		}

		/** Returns a testnet params modified to allow any difficulty target. */
		public static NetworkParameters unitTests() {
			NetworkParameters n = new NetworkParameters();
			n = createTestNet(n);
			n.proofOfWorkLimit = new BigInteger(Hex.Decode("00ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"));
			n.genesisBlock.setNonce(2); // Make this pass the difficulty test 
			n.genesisBlock.setDifficultyTarget(Block.EASIEST_DIFFICULTY_TARGET);
			n.interval = 10;
			n.targetTimespan = 200000000;  // 6 years. Just a very big number.
			n.id = "com.google.bitcoin.unittest";
			return n;
		}

		/**
		 * A java package style string acting as unique ID for these parameters
		 */
		public String getId() {
			if (id == null) {
				// Migrate from old serialized wallets which lack the ID field. This code can eventually be deleted.
				if (port == 8333) {
					id = ID_PRODNET;
				} else if (port == 18333) {
					id = ID_TESTNET;
				}
			}
			return id;
		}

		public bool Equals(Object other) {
			if (!(other is NetworkParameters)) return false;
			NetworkParameters o = (NetworkParameters)other;
			return o.getId().Equals(getId());
		}

		/** Returns the network parameters for the given string ID or NULL if not recognized. */
		public static NetworkParameters fromID(String id) {
			if (id.Equals(ID_PRODNET)) {
				return prodNet();
			} else if (id.Equals(ID_TESTNET)) {
				return testNet();
			} else {
				return null;
			}
		}
	}
}
