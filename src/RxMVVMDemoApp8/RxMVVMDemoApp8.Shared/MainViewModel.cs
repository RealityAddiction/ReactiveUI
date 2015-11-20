using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace RxMVVMDemoApp8
{
	public class MainViewModel : ReactiveObject
	{
		private string _searchTerm;
		public string SearchTerm
		{
			get { return _searchTerm; }
			set { this.RaiseAndSetIfChanged(ref _searchTerm, value); }
		}

		ObservableAsPropertyHelper<List<FlickrPhoto>> _SearchResults;
		public List<FlickrPhoto> SearchResults
		{
			get { return _SearchResults != null ? _SearchResults.Value : new List<FlickrPhoto>(); }
		}

		public ReactiveCommand<object> ClickMe { get; private set; }
		public ReactiveCommand<object> ExecuteSearch { get; protected set; }

		public MainViewModel()
		{
			ExecuteSearch = ReactiveCommand.Create();

			var canClickMeObservable = this.WhenAny(vm => vm.SearchTerm,
				s => !string.IsNullOrWhiteSpace(s.Value));

			ClickMe = ReactiveCommand.Create(canClickMeObservable);
			ClickMe.Subscribe(x => Debug.WriteLine("Clicked"));

			this.ObservableForProperty(x => x.SearchTerm)
				.Throttle(TimeSpan.FromMilliseconds(800), RxApp.TaskpoolScheduler)
				.Select(x => x.Value)
				.DistinctUntilChanged()
				.Where(x => !String.IsNullOrWhiteSpace(x))
				.InvokeCommand(ExecuteSearch);

			IObservable<List<FlickrPhoto>> results;
			results = ExecuteSearch.Select(term => GetSearchResultsFromFlickr((string)term));

			// ...which we then immediately put into the SearchResults Property.
			_SearchResults = results.ToProperty(this, x => x.SearchResults, new List<FlickrPhoto>());
		}

		public static List<FlickrPhoto> GetSearchResultsFromFlickr(string searchTerm)
		{
			HttpClient client = new HttpClient();
			Stream httpDoc = client.GetStreamAsync(String.Format(CultureInfo.InvariantCulture,
				"http://api.flickr.com/services/feeds/photos_public.gne?tags={0}&format=rss_200",
				WebUtility.UrlEncode(searchTerm))).Result;

			var doc = XDocument.Load(httpDoc);

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

