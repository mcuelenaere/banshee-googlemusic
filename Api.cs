using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;

namespace Google.Music
{
	[DataContract]
	class AllTracks
	{
		/*
		 * {"playlistId":"all",
		 *  "requestTime":1327756094241000,
		 *  "continuationToken":"...",
		 *  "differentialUpdate":false,
		 *  "playlist":[...],
		 *  "continuation":false}
		 */
		[DataMember] public string playlistId;
		[DataMember] public long requestTime;
		[DataMember] public string continuationToken;
		[DataMember] public bool differentialUpdate;
		[DataMember] public Track[] playlist;
		[DataMember] public bool continuation;
	}
	
	[DataContract]
	public class Track
	{
		/*
		 * {"genre":"Psychedelic",
		 *  "beatsPerMinute":0,
		 *  "albumArtistNorm":"",
		 *  "artistNorm":"the prodigy",
		 *  "album":"Voodoo People Remixes",
		 *  "lastPlayed":1326606600393895,
		 *  "type":2,
		 *  "disc":0,
		 *  "id":"...",
		 *  "composer":"",
		 *  "title":"Voodoo People ( Parasense remi",
		 *  "albumArtist":"",
		 *  "totalTracks":0,
		 *  "name":"Voodoo People ( Parasense remi",
		 *  "totalDiscs":0,
		 *  "year":1994,
		 *  "titleNorm":"voodoo people ( parasense remi",
		 *  "artist":"The Prodigy",
		 *  "albumNorm":"voodoo people remixes",
		 *  "track":9,
		 *  "durationMillis":534000,
		 *  "deleted":false,
		 *  "url":"",
		 *  "creationDate":1326549559631497,
		 *  "playCount":0,
		 *  "rating":0,
		 *  "comment":""}
		 */
		[DataMember] public string id;
		[DataMember] public string albumArtistNorm;
		[DataMember] public string artistNorm;
		[DataMember] public string albumNorm;
		[DataMember] public string titleNorm;
		[DataMember] public int type;
		[DataMember] public int disc;
		[DataMember] public int totalTracks;
		[DataMember] public int totalDiscs;
		[DataMember] public int year;
		[DataMember] public string composer;
		[DataMember] public string name;
		[DataMember] public string title;
		[DataMember] public string albumArtist;
		[DataMember] public string artist;
		[DataMember] public string album;
		[DataMember] public string genre;
		[DataMember] public int beatsPerMinute;
		[DataMember] public int track;
		[DataMember] public long durationMillis;
		[DataMember] public bool deleted;
		[DataMember] public string url;
		[DataMember] public long creationDate;
		[DataMember] public int playCount;
		[DataMember] public long lastPlayed;
		[DataMember] public int rating;
		[DataMember] public string comment;
	}
	
	[DataContract]
	class PlayUrl
	{
		/*
		 * {"url":"..."}
		 */
		[DataMember] public string url;
	}
	
	[DataContract]
	class ModifyEntries
	{
		/*
		 * {"entries":[...]}
		 */
		[DataMember] public Track[] entries;
	}
	
	[DataContract]
	class ModifyEntriesResult
	{
		/*
		 * {"songs":[...],"success":true}
		 */
		[DataMember] public Track[] songs;
		[DataMember] public bool success;
	}
	
	public class Api
	{
		private const string URL_NEW_AND_RECENT = "https://music.google.com/music/services/newandrecent?u=0&xt={0}";
		private const string URL_LOAD_ALL_TRACKS = "https://music.google.com/music/services/loadalltracks?u=0&xt={0}";
		private const string URL_GET_STATUS = "https://music.google.com/music/services/getstatus?u=0&xt={0}";
		private const string URL_MODIFY_ENTRIES = "https://music.google.com/music/services/modifyentries?u=0&xt={0}";
		private const string URL_PLAY = "https://music.google.com/music/play?u=0&songid={0}&pt=e";
		private const string URL_RECORD_PLAYING = "https://music.google.com/music/services/recordplaying?u=0&xt={0}";

		private CookieCollection cookies = new CookieCollection();
		
		public Api(Cookie[] cookies)
		{
			foreach (Cookie cookie in cookies)
				this.cookies.Add(cookie);
		}
		
		private WebRequest MakeRequest(string url, string postdata)
		{
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
			request.CookieContainer = new CookieContainer();
			request.CookieContainer.Add(cookies);

			if (postdata != null) {
				byte[] data = Encoding.UTF8.GetBytes(postdata);

				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
				request.ContentLength = data.Length;
				
				Stream newStream = request.GetRequestStream();
				newStream.Write(data, 0, data.Length);
				newStream.Close();
			} else {
				request.Method = "GET";
			}
			
			return request;
		}
		
		private string GetXtCookie()
		{
			foreach (Cookie cookie in cookies) {
				if (cookie.Name == "xt")
					return cookie.Value;
			}
			
			return "";
		}
		
		private T Unserialize<T>(Stream stream)
		{
			var serializer = new DataContractJsonSerializer(typeof(T));
			return (T) serializer.ReadObject(stream);
		}
		
		private string Serialize(object obj)
		{
			var serializer = new DataContractJsonSerializer(obj.GetType());
			using (var stream = new MemoryStream()) {
				serializer.WriteObject(stream, obj);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}
		
		private AllTracks GetTracks(string postdata)
		{
			string url = string.Format(URL_LOAD_ALL_TRACKS, GetXtCookie());
			WebRequest request = MakeRequest(url, postdata);
			WebResponse response = request.GetResponse();
			using (var stream = response.GetResponseStream()) {
				var tracks = Unserialize<AllTracks>(stream);
				return tracks;
			}
		}

		private string generateJson(string jsonData)
		{
			return "json=" + HttpUtility.UrlEncode(jsonData);
		}
		
		public Track[] GetTracks()
		{
			var tracks = new List<Track>();
			string continuationToken = null;
			do {
				var postdata = "{" + (continuationToken == null ? "" : "\"continuationToken\":\"" + continuationToken + "\"") + "}";
				var allTracks = GetTracks(generateJson(postdata));
				if (allTracks.playlist != null)
					tracks.AddRange(allTracks.playlist);
				continuationToken = allTracks.continuationToken;
			} while(continuationToken != null);
			
			return tracks.ToArray();
		}
		
		public string PlayTrack(Track track)
		{
			return PlayTrack(track.id);
		}
		
		public string PlayTrack(string trackId)
		{
			string url = string.Format(URL_PLAY, trackId);
			WebRequest request = MakeRequest(url, null);
			WebResponse response = request.GetResponse();
			using (var stream = response.GetResponseStream()) {
				var playUrl = Unserialize<PlayUrl>(stream);
				return playUrl.url;
			}
		}
		
		public bool ModifyEntries(Track[] tracks)
		{
			ModifyEntries modifyEntries = new ModifyEntries();
			modifyEntries.entries = tracks;
			string url = string.Format(URL_MODIFY_ENTRIES, GetXtCookie());
			WebRequest request = MakeRequest(url, "json=" + Serialize(modifyEntries));
			WebResponse response = request.GetResponse();
			using (var stream = response.GetResponseStream()) {
				ModifyEntriesResult result = Unserialize<ModifyEntriesResult>(stream);
				return result.success;
			}
		}
	}
}