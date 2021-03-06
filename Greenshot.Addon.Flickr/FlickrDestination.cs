﻿//  Greenshot - a free and open source screenshot tool
//  Copyright (C) 2007-2017 Thomas Braun, Jens Klingen, Robin Krom
// 
//  For more information see: http://getgreenshot.org/
//  The Greenshot project is hosted on GitHub: https://github.com/greenshot
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 1 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dapplo.HttpExtensions;
using Dapplo.HttpExtensions.Listener;
using Dapplo.HttpExtensions.OAuth;
using Dapplo.Log;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;
using Greenshot.Addon.Core;
using Greenshot.Addon.Interfaces;
using Greenshot.Addon.Interfaces.Destination;
using Greenshot.Addon.Windows;
using Greenshot.CaptureCore;
using Greenshot.Core;
using Greenshot.Core.Interfaces;
using MahApps.Metro.IconPacks;

#endregion

namespace Greenshot.Addon.Flickr
{
	[Destination(FlickrDesignation)]
	public sealed class FlickrDestination : AbstractDestination
	{
		private const string FlickrDesignation = "Flickr";
		private static readonly LogSource Log = new LogSource();
		private static readonly Uri FlickrOAuthUri = new Uri("https://api.flickr.com/services/oauth");
		private OAuth1HttpBehaviour _oAuthHttpBehaviour;

		[Import]
		private IFlickrConfiguration FlickrConfiguration { get; set; }

		[Import]
		private IFlickrLanguage FlickrLanguage { get; set; }

		private async Task<INotification> ExportCaptureAsync(ICapture capture, CancellationToken token = default(CancellationToken))
		{
			var returnValue = new Notification
			{
				NotificationType = NotificationTypes.Success,
				Source = FlickrDesignation,
				SourceType = SourceTypes.Destination,
				Text = string.Format(FlickrLanguage.UploadSuccess, FlickrDesignation)
			};
			var outputSettings = new SurfaceOutputSettings(FlickrConfiguration.UploadFormat, FlickrConfiguration.UploadJpegQuality, false);
			try
			{
				var url = await PleaseWaitWindow.CreateAndShowAsync(Designation, FlickrLanguage.CommunicationWait, async (progress, pleaseWaitToken) =>
				{
					string filename = Path.GetFileName(FilenameHelper.GetFilename(FlickrConfiguration.UploadFormat, capture.CaptureDetails));
					var oAuthHttpBehaviour = _oAuthHttpBehaviour.ShallowClone();
					// Use UploadProgress
					oAuthHttpBehaviour.UploadProgress = percent => { UiContext.RunOn(() => progress.Report((int) (percent*100))); };
					_oAuthHttpBehaviour.MakeCurrent();
					return await FlickrUtils.UploadToFlickrAsync(capture, outputSettings, capture.CaptureDetails.Title, filename, token);
				}, token);

				if (url != null)
				{
					returnValue.ImageLocation = new Uri(url);
					if (FlickrConfiguration.AfterUploadLinkToClipBoard)
					{
						ClipboardHelper.SetClipboardData(url);
					}
				}
			}
			catch (TaskCanceledException tcEx)
			{
				returnValue.Text = string.Format(FlickrLanguage.UploadFailure, FlickrDesignation);
				returnValue.NotificationType = NotificationTypes.Cancel;
				returnValue.ErrorText = tcEx.Message;
				Log.Info().WriteLine(tcEx.Message);
			}
			catch (Exception e)
			{
				returnValue.Text = string.Format(FlickrLanguage.UploadFailure, FlickrDesignation);
				returnValue.NotificationType = NotificationTypes.Fail;
				returnValue.ErrorText = e.Message;
				Log.Warn().WriteLine(e, "Flickr upload gave an exception");
				MessageBox.Show(FlickrLanguage.UploadFailure + " " + e.Message, FlickrDesignation, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			return returnValue;
		}

		/// <summary>
		///     Setup
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
			Designation = FlickrDesignation;
			Export = async (exportContext, capture, token) => await ExportCaptureAsync(capture, token);
			Text = FlickrLanguage.UploadMenuItem;
			Icon = new PackIconFontAwesome
			{
				Kind = PackIconFontAwesomeKind.Flickr
			};

			var oAuthSettings = new OAuth1Settings
			{
				Token = FlickrConfiguration,
				ClientId = FlickrConfiguration.ClientId,
				ClientSecret = FlickrConfiguration.ClientSecret,
				CloudServiceName = "Flickr",
				AuthorizeMode = AuthorizeModes.LocalhostServer,
				TokenUrl = FlickrOAuthUri.AppendSegments("request_token"),
				TokenMethod = HttpMethod.Post,
				AccessTokenUrl = FlickrOAuthUri.AppendSegments("access_token"),
				AccessTokenMethod = HttpMethod.Post,
				AuthorizationUri = FlickrOAuthUri.AppendSegments("authorize")
					.ExtendQuery(new Dictionary<string, string>
					{
						{OAuth1Parameters.Token.EnumValueOf(), "{RequestToken}"},
						{OAuth1Parameters.Callback.EnumValueOf(), "{RedirectUrl}"}
					}),
				// Create a localhost redirect uri, prefer port 47336, but use the first free found
				RedirectUrl = new[] {47336, 0}.CreateLocalHostUri().AbsoluteUri,
				CheckVerifier = true
			};

			_oAuthHttpBehaviour = OAuth1HttpBehaviourFactory.Create(oAuthSettings);
			_oAuthHttpBehaviour.ValidateResponseContentType = false;
		}
	}
}