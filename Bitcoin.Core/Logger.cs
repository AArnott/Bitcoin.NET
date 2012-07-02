namespace Bitcoin.Core {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using log4net;

	internal static class Logger {
		private static ILog log = log4net.LogManager.GetLogger("Bitcoin");

		internal static ILog Log {
			get { return log; }
		}
	}
}
