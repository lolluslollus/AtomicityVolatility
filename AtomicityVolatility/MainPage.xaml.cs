using AtomicityVolatility.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Utilz.Controlz;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409
// LOLLO NOTE debug this with "optimise code" and "native tool chain"
// LOLLO NOTE these examples are based on while() ;
// it hogs the thread, which never gets a chance to get the new values.
// if you use 
// while () await Task.Delay(50);
// the thread spins a couple of times and then gets updated.
// Still, this delay can be critical in some cases. After measuring the performance, it looks like the volatile keyword is the most efficient solution.


namespace AtomicityVolatility
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : ObservablePage
	{
		private bool _isGetOut = false;

		private bool _boolOfMain = false;
		public bool BoolOfMainVolatileRead { get { return Volatile.Read(ref _boolOfMain); } set { _boolOfMain = value; } }
		public bool BoolOfMainVolatileReadWrite { get { return Volatile.Read(ref _boolOfMain); } set { Volatile.Write(ref _boolOfMain, value); } }
		public bool BoolOfMainProperty { get { return _boolOfMain; } set { _boolOfMain = value; } }
		public bool BoolOfMainObservableProperty { get { return _boolOfMain; } set { _boolOfMain = value; RaisePropertyChanged_UI(); } }
		public volatile bool BoolOfMainVolatile = false;
		private static bool BoolOfMainStatic = false;

		private Persistent _persistent = null;
		public Persistent Persistent { get { return _persistent; } private set { _persistent = value; RaisePropertyChanged_UI(); } }


		public MainPage()
		{
			Persistent = new Persistent();
			InitializeComponent();
		}

		private async void OnButton0_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			BoolOfMainProperty = false;
			var checker = new StatusCheckerWithRefToMain(this);

			//await checker.UpdateStatus().ConfigureAwait(false); // this one updates and we can see the result here
			//Task www = checker.UpdateStatus(); // this one hangs forever
			Task.Run(delegate { return checker.UpdateStatus(); }); // this one hangs forever


			while (!BoolOfMainProperty || _isGetOut) ; // { await Task.Delay(50); }

			MakeBtnGreen(sender);
		}

		private async void OnButton1_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			_persistent.BoolObservable = false;
			var checker = new StatusCheckerWithRefToPersistent(_persistent);

			//await checker.UpdateStatus().ConfigureAwait(false); // this one updates and we can see the result here
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return checker.UpdateStatus(); }); // this works

			while (!_persistent.BoolObservable || _isGetOut) ;

			MakeBtnGreen(sender);
		}

		private async void OnButton2_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			BoolOfMainVolatile = false;
			var checker = new StatusCheckerWithRef_VolatileOwner(this);

			//await checker.UpdateStatus().ConfigureAwait(false); // this one updates and we can see the result here
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return checker.UpdateStatus(); }); // this works


			while (!BoolOfMainVolatile || _isGetOut) ;

			MakeBtnGreen(sender);
		}

		private async void OnButton3_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			var checker = new StatusCheckerVolatile();
			checker.StatusUpdated = false;

			//await checker.UpdateStatus().ConfigureAwait(false); // this one does not hang and sees the right value
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return checker.UpdateStatus(); }); // this works

			while (!checker.StatusUpdated || _isGetOut) ;

			MakeBtnGreen(sender);
		}
		private async void OnButton4_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			var checker = new StatusChecker();
			checker.StatusUpdated = false;

			//await checker.UpdateStatus().ConfigureAwait(false); // this one does not hang and sees the right value
			//Task www = checker.UpdateStatus(); // this one hangs forever
			Task.Run(delegate { return checker.UpdateStatus(); }); // this one hangs forever

			while (!checker.StatusUpdated || _isGetOut) ;

			MakeBtnGreen(sender);
		}
		private async void OnButton5_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			var checker = new StatusCheckerVolatileRead();
			//checker.StatusUpdated = false;

			//await checker.UpdateStatus().ConfigureAwait(false); // this one does not hang and sees the right value
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return checker.UpdateStatus(); }); // this works

			while (!checker.StatusUpdated || _isGetOut) ;

			MakeBtnGreen(sender);
		}

		private async void OnButton6_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			var checker = new StatusCheckerVolatileLock();
			//checker.StatusUpdated = false;

			//await checker.UpdateStatus().ConfigureAwait(false); // this one does not hang and sees the right value
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return checker.UpdateStatus(); }); // this works

			while (!checker.StatusUpdated || _isGetOut) ;

			MakeBtnGreen(sender);
		}
		private async void OnButton7_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);
			// this fails sometimes, it's most interesting.
			BoolOfMainStatic = false;
			_persistent.BoolObservable = false;
			
			_boolOfMain = false;

			Task away = Task.Run(async delegate
		   {
			   await Task.Delay(500).ConfigureAwait(false);

			   //BoolOfMainStatic = true;
			   //_persistent.Bool0 = true;
			   //BoolOfMainProperty = true; // fails
			   //_boolOfMain = true; // fails
			   //BoolOfMainVolatile = true;
			   //BoolOfMainVolatileReadWrite = true;
			   //BoolOfMainVolatileRead = true;
			   BoolOfMainObservableProperty = true; // fails

			   Debug.WriteLine("updates done");
		   });

			while (!BoolOfMainObservableProperty || _isGetOut) ; // this hangs forever, and the binding does not pull, either.

			//while (!BoolOfMainStatic) ; // this works
			//Debugger.Break();
			//while (!_persistent.Bool0) ; // this works
			//Debugger.Break();

			//while (!_boolOfMain) ; // this hangs forever; however, 
			// if true is READ after reading a volatile or static property, then it works, 
			// never mind if the volatile or static property is set before or after it.
			// this cannot be reproduced 100%, except that it always hangs when BoolOfMain is written and read alone.
			// if it is read first, it always hangs.

			//while (!BoolOfMainProperty) ; // this hangs forever ; however, 
								  // if true is READ after reading a volatile or static property, then it works, 
								  // never mind if the volatile or static property is set before or after it.
								  // this cannot be reproduced 100%, except that it always hangs when BoolOfMain is written and read alone.
								  // if it is read first, it always hangs.

			// it works at home though...

			//Debugger.Break();
			//while (!BoolOfMainVolatile) ; // this works
			//Debugger.Break();
			//while (!BoolOfMainVolatileReadWrite) ; // this works
			//Debugger.Break();
			//while (!BoolOfMainVolatileRead) ; // this works

			MakeBtnGreen(sender);
		}
		private async void OnButton8_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			_persistent.BoolObservable = false;

			//await checker.UpdateStatus().ConfigureAwait(false); // this one does not hang and sees the right value
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return _persistent.MakeTrueAsync(); }); // this hangs forever

			while (!_persistent.BoolObservable || _isGetOut) ; // { await Task.Delay(50); }

			MakeBtnGreen(sender);
		}

		private async void OnButton9_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			_persistent.BoolLocked = false;

			//await checker.UpdateStatus().ConfigureAwait(false); // this one does not hang and sees the right value
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return _persistent.MakeTrueAsync(); }); // this works

			while (!_persistent.BoolLocked || _isGetOut) ; // await Task.Delay(50);

			MakeBtnGreen(sender);
		}
		private async void OnButton10_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			_persistent.BoolVolatile = false;

			//await checker.UpdateStatus().ConfigureAwait(false); // this one does not hang and sees the right value
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return _persistent.MakeTrueAsync(); }); // this works

			while (!_persistent.BoolVolatile || _isGetOut) ; // await Task.Delay(50);

			MakeBtnGreen(sender);
		}
		private async void OnButton11_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			_persistent.BoolVolatileRead = false;

			//await checker.UpdateStatus().ConfigureAwait(false); // this one does not hang and sees the right value
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return _persistent.MakeTrueAsync(); }); // this works

			while (!_persistent.BoolVolatileRead || _isGetOut) ; // await Task.Delay(50);

			MakeBtnGreen(sender);
		}

		private async void OnButtonPerf_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);
			bool test = false;
			Stopwatch sw = new Stopwatch();
			int ii = 100000000;

			for (int i = 0; i < ii; i++)
			{
				test = _persistent.BoolLocked; test = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolLocked gets took " + sw.ElapsedMilliseconds + " msec");

			sw.Restart();
			for (int i = 0; i < ii; i++)
			{
				test = _persistent.BoolObservable; test = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolObservable gets took " + sw.ElapsedMilliseconds + " msec");

			sw.Restart();
			for (int i = 0; i < ii; i++)
			{
				test = _persistent.BoolVolatile; test = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolVolatile gets took " + sw.ElapsedMilliseconds + " msec");

			sw.Restart();
			for (int i = 0; i < ii; i++)
			{
				test = _persistent.BoolVolatileRead; test = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolVolatileRead gets took " + sw.ElapsedMilliseconds + " msec");

			ii = 10000000;
			sw.Restart();
			for (int i = 0; i < ii; i++)
			{
				_persistent.BoolLocked = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolLocked sets took " + sw.ElapsedMilliseconds + " msec");

			sw.Restart();
			for (int i = 0; i < ii; i++)
			{
				_persistent.BoolObservable = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolObservable sets took " + sw.ElapsedMilliseconds + " msec");

			sw.Restart();
			for (int i = 0; i < ii; i++)
			{
				_persistent.BoolVolatile = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolVolatile sets took " + sw.ElapsedMilliseconds + " msec");

			sw.Restart();
			for (int i = 0; i < ii; i++)
			{
				_persistent.BoolVolatileRead = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolVolatileRead sets took " + sw.ElapsedMilliseconds + " msec");

			sw.Restart();
			for (int i = 0; i < ii; i++)
			{
				_persistent.BoolVolatileReadWrite = !test;
			}
			sw.Stop();
			Debug.WriteLine("BoolVolatileReadWrite sets took " + sw.ElapsedMilliseconds + " msec");

			MakeBtnGreen(sender);
		}

		
		#region utilz
		private void MakeBtnRed(object sender)
		{
			_isGetOut = false;
			Task ss = RunInUiThreadAsync(delegate
			{
				(sender as Button).Foreground = new SolidColorBrush(Colors.Red);
			});
		}
		private void MakeBtnGreen(object sender)
		{
			Task ss = RunInUiThreadAsync(delegate
			{
				(sender as Button).Foreground = new SolidColorBrush(Colors.Green);
			});
		}


		class StatusChecker
		{
			public string Result;
			public bool StatusUpdated;

			public async Task UpdateStatus()
			{
				await Task.Delay(2000).ConfigureAwait(false);
				Result = "OK";
				Debug.WriteLine("Status Obtained");
				StatusUpdated = true;
			}
		}
		class StatusCheckerVolatile
		{
			public string Result;
			public volatile bool StatusUpdated;

			public async Task UpdateStatus()
			{
				await Task.Delay(2000).ConfigureAwait(false);
				Result = "OK";
				Debug.WriteLine("Status Obtained");
				StatusUpdated = true;
			}
		}
		class StatusCheckerVolatileRead
		{
			public string Result;
			private bool _statusUpdated = false;
			public bool StatusUpdated { get { return Volatile.Read(ref _statusUpdated); } }

			public async Task UpdateStatus()
			{
				await Task.Delay(2000).ConfigureAwait(false);
				Result = "OK";
				Debug.WriteLine("Status Obtained");
				_statusUpdated = true;
			}
		}
		class StatusCheckerVolatileLock
		{
			private readonly object _locker = new object();
			public string Result;
			private bool _statusUpdated = false;
			public bool StatusUpdated
			{
				get
				{
					lock (_locker) // no locker will break it
					{
						return _statusUpdated;
					}
				}
			}

			public async Task UpdateStatus()
			{
				await Task.Delay(2000).ConfigureAwait(false);
				Result = "OK";
				Debug.WriteLine("Status Obtained");
				_statusUpdated = true;
			}
		}
		class StatusCheckerWithRefToMain
		{
			private MainPage _owner = null;
			public string Result;

			public StatusCheckerWithRefToMain(MainPage owner)
			{
				_owner = owner;
			}

			public async Task UpdateStatus()
			{
				await Task.Delay(2000).ConfigureAwait(false);
				Result = "OK";
				Debug.WriteLine("Status Obtained");
				_owner.BoolOfMainProperty = true;
			}
		}
		class StatusCheckerWithRefToPersistent
		{
			private Persistent _owner = null;
			public string Result;

			public StatusCheckerWithRefToPersistent(Persistent owner)
			{
				_owner = owner;
			}

			public async Task UpdateStatus()
			{
				await Task.Delay(2000).ConfigureAwait(false);
				Result = "OK";
				Debug.WriteLine("Status Obtained");
				_owner.BoolObservable = true;
			}
		}
		class StatusCheckerWithRef_VolatileOwner
		{
			private MainPage _owner = null;
			public string Result;

			public StatusCheckerWithRef_VolatileOwner(MainPage owner)
			{
				_owner = owner;
			}

			public async Task UpdateStatus()
			{
				await Task.Delay(2000).ConfigureAwait(false);
				Result = "OK";
				Debug.WriteLine("Status Obtained");
				_owner.BoolOfMainVolatile = true;
			}
		}
		#endregion utilz

		private void OnGetOut_Click(object sender, RoutedEventArgs e)
		{
			_isGetOut = true;
		}
	}
}