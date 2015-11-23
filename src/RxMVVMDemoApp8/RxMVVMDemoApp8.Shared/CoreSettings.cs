using BlueKiwi.SDK.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RxMVVMDemoApp8
{
	public class CoreSettings : ISdkSettings
	{
		public CoreSettings()
		{

		}

		public void Save()
		{

		}

		public string BaseUri
		{
			get
			{
				return "https://partners-beta.sandboxbk.net/";
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string ClientId
		{
			get
			{
				return "543b8c8fd7c005e32c26";
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string ClientSecret
		{
			get
			{
				return "26c7bb26b79e6c822cfa22e9f2fe1a9b";
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
}
