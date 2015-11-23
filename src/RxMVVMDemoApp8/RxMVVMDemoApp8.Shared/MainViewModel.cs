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
using BlueKiwi.SDK.Models;
using BlueKiwi.SDK.ServiceAgents.Request;
using BlueKiwi.SDK.Contracts.Services;
using BlueKiwi.SDK.Services;
using BlueKiwi.SDK.ServiceAgents;
using BlueKiwi.SDK.Contracts;
using BlueKiwi.SDK.Contracts.ServiceAgents;
using BlueKiwi.SDK.Contracts.Domain;
using BlueKiwi.SDK.Domain;
using BlueKiwi.SDK.ServiceAgents.Response.Me;
using System.Threading;
using BlueKiwi.SDK.ServiceAgents.Response;
using BlueKiwi.SDK.ServiceAgents.Response.Feed;
using System.Reactive.Disposables;


namespace RxMVVMDemoApp8
{
	public class MainViewModel : ReactiveObject
	{
		private const string _accessToken = "bc6921b2ca5144fc050e163dc52a5cc3";
		private IPagingFilter _paging;
		private IRestUserApiServiceAgent _userAgent;
		public IUserService _userService;	
		public IBlueKiwiIdentity _identity;

		private bool _isLoading;

		public bool IsLoading
		{
			get { return _isLoading; }
			set { this.RaiseAndSetIfChanged(ref _isLoading, value); }
		}
		

		public ReactiveList<Post> SearchResults { get; set; }
		public ReactiveCommand<Post> ExecuteSearch { get; protected set; }

		public MainViewModel()
		{
			SearchResults = new ReactiveList<Post>();
			var canExecute = this.WhenAny(x => x.IsLoading, x => !x.GetValue() );

			_paging = new Paging() { Limit = 20, Offset = 0 };

			//ISdkSettings settings = new CoreSettings();
			//_authAgent = new OAuth2ServiceAgent(settings);

			_identity = new BlueKiwiIdentity();
			_identity.InstanceUrl = "https://partners-beta.sandboxbk.net";
			_identity.User = new MeResponse() { 
				Avatar = "https://partners-beta.sandboxbk.net/cache/partnersbeta/user/avatar/7b37cd0595f0793baa25c3546608ded5_avatar_small.jpg?ttl=1430397197",
				Id = 1689,
				Login = "ibs"
			};

			_identity.Token = new BlueKiwiOAuthToken() { AccessToken = "a12b2b7511281a98ac07cdcd0ddb3a26" };


			_userAgent = new RestUserApiServiceAgent(_identity);
			_userService = new UserService(_userAgent);

			ExecuteSearch = ReactiveCommand.CreateAsyncObservable<Post>(canExecute, _ => GetPosts().Finally(Final));

			ExecuteSearch.Subscribe(OnNext,new CancellationToken());
		}

		private void Final()
		{
			IsLoading = false;
		}

		private void OnNext(Post obj)
		{
			SearchResults.Add(obj);
		}

		public IObservable<Post> GetPosts()
		{
			IsLoading = true;
			return Observable.Create<Post>(async (IObserver<Post> observer) => {	
				Response<FeedObject> call = await _userService.GetFeed((int)_identity.User.Id, "_discussionFeed", _paging, new CancellationToken());

				if (call.Items == null || !call.Items.Any()) observer.OnCompleted();

				_paging.Offset += 20;

				foreach (FeedObject obj in call.Items)
				{
					observer.OnNext(new Post() { Url = obj.Author.Avatar, Title = obj.Object.Name, Description = obj.Object.Name });
					await Task.Delay(500);
				}
				Debug.WriteLine("Before OnCompleted");
				observer.OnCompleted();

				return Disposable.Create(() => 
				{
					Debug.WriteLine("Observer has unsubscribed"); 
				});
			});
		}
	}
}


