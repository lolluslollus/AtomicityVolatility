using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilz.Data;

namespace AtomicityVolatility.Data
{
	public class Persistent : ObservableData
	{
		private bool _boolObservable = false;
		public bool BoolObservable { get { return _boolObservable; } set { _boolObservable = value; /*RaisePropertyChanged_UI();*/ } } // this fails misteriously

		private bool _boolLocked = false; private readonly object _boolLocker = new object();
		public bool BoolLocked
		{
			get
			{
				lock (_boolLocker)
				{
					return _boolLocked;
				}
			}
			set
			{
				lock (_boolLocker)
				{
					_boolLocked = value;
				}
			}
		}

		private volatile bool _boolVolatile = false; private readonly object _boolVoLo = new object();
		public bool BoolVolatile
		{
			get { return _boolVolatile; }
			set
			{
				//lock (_boolVoLo)
				{
					_boolVolatile = value;
				}
			}
		}

		private bool _boolVolatileRead = false; private readonly object _boolVoLoRe = new object();
		public bool BoolVolatileRead
		{
			get { return Volatile.Read(ref _boolVolatileRead); }
			set
			{
				//lock (_boolVoLoRe)
				{
					_boolVolatileRead = value;
				}
			}
		}

		private bool _boolVolatileReadWrite = false; private readonly object _boolVoLoReWr = new object();
		public bool BoolVolatileReadWrite
		{
			get { return Volatile.Read(ref _boolVolatileReadWrite); }
			set
			{
				//lock (_boolVoLoReWr)
				{
					Volatile.Write(ref _boolVolatileReadWrite, value);
				}
			}
		}

		private byte _byte0 = 0;
		public byte Byte0 { get { return _byte0; } set { _byte0 = value; } }

		private byte _byte1 = 0;
		public byte Byte1 { get { return _byte1; } set { _byte1 = value; } }

		private byte _byte2 = 0;
		public byte Byte2 { get { return _byte2; } set { _byte2 = value; } }

		public async Task MakeTrueAsync()
		{
			await Task.Delay(1000).ConfigureAwait(false);
			BoolObservable = true;
			BoolLocked = true;
			BoolVolatile = true;
			BoolVolatileRead = true;
			Debug.WriteLine("MakeTrueAsync() set all to true");
		}

	}
}
