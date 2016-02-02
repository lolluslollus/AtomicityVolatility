using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilz.Data;

namespace AtomicityVolatility.Data
{
	public class Persistent : ObservableData
	{
		private bool _bool0 = false;
		public bool Bool0 { get { return _bool0; } set { _bool0 = value; } }

		private bool _bool1 = false;
		public bool Bool1 { get { return _bool1; } set { _bool1 = value; } }

		private bool _bool2 = false;
		public bool Bool2 { get { return _bool2; } set { _bool2 = value; } }

		private byte _byte0 = 0;
		public byte Byte0 { get { return _byte0; } set { _byte0 = value; } }

		private byte _byte1 = 0;
		public byte Byte1 { get { return _byte1; } set { _byte1 = value; } }

		private byte _byte2 = 0;
		public byte Byte2 { get { return _byte2; } set { _byte2 = value; } }

	}
}
