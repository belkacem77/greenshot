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

using System.ComponentModel;
using System.Runtime.Serialization;
using Dapplo.Config.Ini;
using Dapplo.HttpExtensions.OAuth;
using Greenshot.Core.Configuration;

#endregion

namespace Greenshot.Addon.Flickr
{
	public enum SafetyLevel
	{
		Safe = 1,
		Moderate = 2,
		Restricted = 3
	}

	/// <summary>
	///     Description of FlickrConfiguration.
	/// </summary>
	[IniSection("Flickr")]
	[Description("Greenshot Flickr Plugin configuration")]
	public interface IFlickrConfiguration : IOAuth1Token, IIniSection<IFlickrConfiguration>
	{
		[Description("After upload send flickr link to clipboard.")]
		[DefaultValue(true)]
		bool AfterUploadLinkToClipBoard { get; set; }

		/// <summary>
		///     Not stored, but read so people could theoretically specify their own Client ID.
		/// </summary>
		[IniPropertyBehavior(Write = false)]
		[DefaultValue("@credentials_flickr_consumer_key@")]
		string ClientId { get; set; }

		/// <summary>
		///     Not stored, but read so people could theoretically specify their own client secret.
		/// </summary>
		[IniPropertyBehavior(Write = false)]
		[DefaultValue("@credentials_flickr_consumer_secret@")]
		string ClientSecret { get; set; }

		[Description("Hidden from search")]
		[DefaultValue(false)]
		bool HiddenFromSearch { get; set; }

		[DataMember(Name = "flickrIsFamily")]
		[Description("IsFamily.")]
		[DefaultValue(true)]
		bool IsFamily { get; set; }

		[DataMember(Name = "flickrIsFriend")]
		[Description("IsFriend.")]
		[DefaultValue(true)]
		bool IsFriend { get; set; }

		[DataMember(Name = "flickrIsPublic")]
		[Description("IsPublic.")]
		[DefaultValue(true)]
		bool IsPublic { get; set; }

		[Description("Safety level")]
		[DefaultValue(SafetyLevel.Safe)]
		SafetyLevel SafetyLevel { get; set; }

		[Description("What file type to use for uploading")]
		[DefaultValue(OutputFormat.png)]
		OutputFormat UploadFormat { get; set; }

		[Description("JPEG file save quality in %.")]
		[DefaultValue(80)]
		int UploadJpegQuality { get; set; }

		[Description("Use pagelink instead of direct link on the clipboard")]
		[DefaultValue(false)]
		bool UsePageLink { get; set; }
	}
}