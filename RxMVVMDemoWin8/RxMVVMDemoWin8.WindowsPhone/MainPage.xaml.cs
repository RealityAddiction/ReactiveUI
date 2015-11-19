using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace RxMVVMDemoWin8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Since we're using the MVVM pattern, we're going to bind to our 
        // ViewModel object in the code, and our View code-behind will be
        // concerned only with things that are solely view-based, like 
        // minimizing/maximizing a window
        public AppViewModel ViewModel { get; protected set; }

        public MainPage()
        {
            ViewModel = new AppViewModel();
            InitializeComponent();
        }
    }

    // Create a simple model class to store our Flickr results - since we will 
    // never update the properties once we create the object, we don't have to
    // use ReactiveObject, just good-old auto-properties.
    public class FlickrPhoto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }

    // AppViewModel is where we will describe the interaction of our application
    // (we can describe the entire application in one class since this is very 
    // small). 
    public class AppViewModel : ReactiveObject
    {
        // In ReactiveUI, this is the syntax to declare a read-write property
        // that will notify Observers (as well as WPF) that a property has 
        // changed. If we declared this as a normal property, we couldn't tell 
        // when it has changed!

        string _SearchTerm;
        public string SearchTerm
        {
            get { return _SearchTerm; }
            set { this.RaiseAndSetIfChanged(ref _SearchTerm, value); }
        }

        // We will describe this later, but ReactiveAsyncCommand is a Command
        // (like "Open", "Copy", "Delete", etc), that manages a task running
        // in the background.

        public ReactiveCommand<object> ExecuteSearch { get; protected set; }


        /* ObservableAsPropertyHelper
         * 
         * Here's the interesting part: In ReactiveUI, we can take IObservables
         * and "pipe" them to a Property - whenever the Observable yields a new
         * value, we will notify ReactiveObject that the property has changed.
         * 
         * To do this, we have a class called ObservableAsPropertyHelper - this
         * class subscribes to an Observable and stores a copy of the latest value.
         * It also runs an action whenever the property changes, usually calling
         * ReactiveObject's RaisePropertyChanged.
         */

        ObservableAsPropertyHelper<List<FlickrPhoto>> _SearchResults;
        public List<FlickrPhoto> SearchResults
        {
            get { return _SearchResults.Value; }
        }

        // Here, we want to create a property to represent when the application 
        // is performing a search (i.e. when to show the "spinner" control that 
        // lets the user know that the app is busy). We also declare this property
        // to be the result of an Observable (i.e. its value is derived from 
        // some other property)

        ObservableAsPropertyHelper<Visibility> _SpinnerVisibility;
        public Visibility SpinnerVisibility
        {
            get { return _SpinnerVisibility.Value; }
        }

        public AppViewModel(ReactiveCommand<object> testExecuteSearchCommand = null, IObservable<List<FlickrPhoto>> testSearchResults = null)
        {
            ExecuteSearch = testExecuteSearchCommand ?? ReactiveCommand.Create();

            this.ObservableForProperty(x => x.SearchTerm)
                .Throttle(TimeSpan.FromMilliseconds(800), RxApp.TaskpoolScheduler)
                .Select(x => x.Value)
                .DistinctUntilChanged()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .InvokeCommand(ExecuteSearch);

            _SpinnerVisibility = ExecuteSearch.IsExecuting.Select(x => x ? Visibility.Visible : Visibility.Collapsed)
                .ToProperty(this, x => x.SpinnerVisibility, Visibility.Collapsed);

            IObservable<List<FlickrPhoto>> results;
            if (testSearchResults != null)
            {
                results = testSearchResults;
            }
            else
            {
                results = ExecuteSearch.Select(term => GetSearchResultsFromFlickr((string)term));
            }

            _SearchResults = results.ToProperty(this, x => x.SearchResults, new List<FlickrPhoto>());
        }

        public static List<FlickrPhoto> GetSearchResultsFromFlickr(string searchTerm)
        {
            var doc = XDocument.Load(String.Format(CultureInfo.InvariantCulture,
                "http://api.flickr.com/services/feeds/photos_public.gne?tags={0}&format=rss_200",
                WebUtility.UrlEncode(searchTerm)));

            if (doc.Root == null)
                return null;

            var titles = doc.Root.Descendants("{http://search.yahoo.com/mrss/}title")
                .Select(x => x.Value);

            var tagRegex = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            var descriptions = doc.Root.Descendants("{http://search.yahoo.com/mrss/}description")
                .Select(x => tagRegex.Replace(WebUtility.HtmlDecode(x.Value), ""));

            var items = titles.Zip(descriptions,
                (t, d) => new FlickrPhoto { Title = t, Description = d }).ToArray();

            var urls = doc.Root.Descendants("{http://search.yahoo.com/mrss/}thumbnail")
                .Select(x => x.Attributes("url").First().Value);

            var ret = items.Zip(urls, (item, url) => { item.Url = url; return item; }).ToList();
            return ret;
        }
    }
}
