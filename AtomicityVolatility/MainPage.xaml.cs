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
// LOLLO NOTE debug this with "optimise code"

namespace AtomicityVolatility
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : ObservablePage
	{
		private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		private bool _boolOfMain = false;
		public bool BoolOfMainVolatileRead { get { return Volatile.Read(ref _boolOfMain); } set { _boolOfMain = value; } }
		public bool BoolOfMainVolatileReadWrite { get { return Volatile.Read(ref _boolOfMain); } set { Volatile.Write(ref _boolOfMain, value); } }
		public bool BoolOfMain = false;
		public bool BoolOfMainSemaphore
		{
			get
			{
				try
				{
					_semaphore.Wait();
					return _boolOfMain;
				}
				finally
				{
					_semaphore.Release();
				}
			}
			set
			{
				try
				{
					_semaphore.Wait();
					_boolOfMain = value;
				}
				finally
				{
					_semaphore.Release();
				}
			}
		}
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

			BoolOfMain = false;
			var checker = new StatusCheckerWithRefToMain(this);

			//await checker.UpdateStatus().ConfigureAwait(false); // this one updates and we can see the result here
			//Task www = checker.UpdateStatus(); // this one hangs forever
			Task.Run(delegate { return checker.UpdateStatus(); }); // this one hangs forever


			while (!BoolOfMain) ;

			MakeBtnGreen(sender);
		}
		private async void OnButton1_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);

			_persistent.Bool0 = false;
			var checker = new StatusCheckerWithRefToPersistent(_persistent);

			//await checker.UpdateStatus().ConfigureAwait(false); // this one updates and we can see the result here
			//Task www = checker.UpdateStatus(); // this works
			Task.Run(delegate { return checker.UpdateStatus(); }); // this works

			while (!_persistent.Bool0) ;

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


			while (!BoolOfMainVolatile) ;

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

			while (!checker.StatusUpdated) ;

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

			while (!checker.StatusUpdated) ;

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

			while (!checker.StatusUpdated) ;

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

			while (!checker.StatusUpdated) ;

			MakeBtnGreen(sender);
		}
		private async void OnButton7_Click(object sender, RoutedEventArgs e)
		{
			MakeBtnRed(sender);
			// this fails sometimes, it's most interesting.
			BoolOfMainStatic = false;
			_persistent.Bool0 = false;
			BoolOfMain = false;
			BoolOfMainVolatile = false;
			BoolOfMainVolatileReadWrite = false;
			BoolOfMainVolatileRead = false;
			BoolOfMainSemaphore = false;

			Task away = Task.Run(async delegate
		   {
			   await Task.Delay(500).ConfigureAwait(false);

			   //BoolOfMainStatic = true;
			   //_persistent.Bool0 = true;
			   //BoolOfMain = true;
			   //BoolOfMainVolatile = true;
			   //BoolOfMainVolatileReadWrite = true;
			   //BoolOfMainVolatileRead = true;
			   BoolOfMainSemaphore = true;

			   Debug.WriteLine("updates done");
		   });

			while (!BoolOfMainSemaphore) ; // this works

			//while (!BoolOfMainStatic) ; // this works
			//Debugger.Break();
			//while (!_persistent.Bool0) ; // this works
			//Debugger.Break();
			//while (!BoolOfMain) ; // this hangs forever; however, 
			//					  // if true is READ after reading a volatile or static property, then it works, 
			//					  // never mind if the volatile or static property is set before or after it.
			//					  // this cannot be reproduced 100%, except that it always hangs when BoolOfMain is written and read alone.
			//					  // if it is read first, it always hangs.

			//Debugger.Break();
			//while (!BoolOfMainVolatile) ; // this works
			//Debugger.Break();
			//while (!BoolOfMainVolatileReadWrite) ; // this works
			//Debugger.Break();
			while (!BoolOfMainVolatileRead) ; // this works

			MakeBtnGreen(sender);
		}


		#region utilz
		private void MakeBtnRed(object sender)
		{
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
				_owner.BoolOfMain = true;
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
				_owner.Bool0 = true;
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
	}
}