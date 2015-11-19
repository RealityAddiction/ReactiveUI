using ReactiveUI;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text.RegularExpressions;
using System.Globalization;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RxMVVMDemoApp8
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainPage : Page
	{
		// Since we're using the MVVM pattern, we're going to bind to our 
		// ViewModel object in the code, and our View code-behind will be
		// concerned only with things that are solely view-based, like 
		// minimizing/maximizing a window
		public MainViewModel ViewModel { get; protected set; }

		public MainPage()
		{
			InitializeComponent();
			ViewModel = new MainViewModel();
			this.DataContext = ViewModel;
		}
	}
}
