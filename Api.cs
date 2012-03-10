using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;

using Hyena.Json;

namespace Google.Music
{
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
		[DataMember] public string albumDataUrl;
		[DataMember] public string url;
		[DataMember] public long creationDate;
		[DataMember] public int playCount;
		[DataMember] public long lastPlayed;
		[DataMember] public int rating;
		[DataMember] public string comment;
	}

	class Json
	{
		private static readonly Deserializer deserializer = new Deserializer();
		private static readonly Serializer serializer = new Serializer();
		
		public static IJsonCollection Unserialize(Stream stream)
		{
			deserializer.SetInput(stream);
			return (IJsonCollection) deserializer.Deserialize();
		}
		
		public static T Unserialize<T>(Stream stream)
		{
			var serializer = new DataContractJsonSerializer(typeof(T));
			return (T) serializer.ReadObject(stream);
		}
		
		public static T Convert<T>(IJsonCollection obj)
		{
			/* TODO: can this be done more efficiently? */
			var serializer = new DataContractJsonSerializer(typeof(T));
			var textData = obj.ToString();
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(textData))) {
				return (T) serializer.ReadObject(stream);
			}
		}
		
		public static string Serialize(object obj)
		{
			var serializer = new DataContractJsonSerializer(obj.GetType());
			using (var stream = new MemoryStream()) {
				serializer.WriteObject(stream, obj);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}
		
		public static string Serialize(IJsonCollection obj)
		{
			serializer.SetInput(obj);
			return serializer.Serialize();
		}
	}
	
	public class Api
	{
		private const string URL_NEW_AND_RECENT = "https://play.google.com/music/services/newandrecent?u=0&xt={0}";
		private const string URL_LOAD_ALL_TRACKS = "https://play.google.com/music/services/loadalltracks?u=0&xt={0}";
		private const string URL_GET_STATUS = "https://play.google.com/music/services/getstatus?u=0&xt={0}";
		private const string URL_MODIFY_ENTRIES = "https://play.google.com/music/services/modifyentries?u=0&xt={0}";
		private const string URL_PLAY = "https://play.google.com/music/play?u=0&songid={0}&pt=e";
		private const string URL_RECORD_PLAYING = "https://play.google.com/music/services/recordplaying?u=0&xt={0}";

		private CookieCollection cookies = new CookieCollection();
		
		public Api()
		{
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
		
		private string generateJson(IJsonCollection jsonData)
		{
			return "json=" + HttpUtility.UrlEncode(Json.Serialize(jsonData));
		}
		
		public void SetCookies(Cookie[] cookies)
		{
			foreach (Cookie cookie in cookies)
				this.cookies.Add(cookie);
		}
		
		private JsonObject GetTracks(string postdata)
		{
			string url = string.Format(URL_LOAD_ALL_TRACKS, GetXtCookie());
			WebRequest request = MakeRequest(url, postdata);
			WebResponse response = request.GetResponse();
			using (var stream = response.GetResponseStream())
				return (JsonObject) Json.Unserialize(stream);
		}
		
		public IEnumerable<Track> GetTracks()
		{
			var loopCount = 30; /* don't get stuck in an infinite loop */
			string continuationToken = null;
			do {
				var postdata = new JsonObject();
				if (continuationToken != null)
					postdata.Add("continuationToken", continuationToken);
				
				var allTracks = GetTracks(generateJson(postdata));
				if (allTracks.ContainsKey("playlist")) {
					var playlist = (JsonArray) allTracks["playlist"];
					foreach (var track in playlist)
						yield return Json.Convert<Track>((JsonObject) track);
				}
				
				if (allTracks.ContainsKey("continuationToken"))
					continuationToken = (string) allTracks["continuationToken"];
				else
					continuationToken = null;
			} while(!string.IsNullOrEmpty(continuationToken) && loopCount-- > 0);
		}
		
		public Track[] GetTracksArray()
		{
			var tracks = new List<Track>();
			foreach (var track in GetTracks())
				tracks.Add(track);
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
				var playUrl = (JsonObject) Json.Unserialize(stream);
				if (playUrl.ContainsKey("url"))
					return (string) playUrl["url"];
				else
					return null;
			}
		}
		
		public bool ModifyEntries(Track[] tracks)
		{
			var modifyEntries = new JsonArray();
			foreach (var track in tracks)
				modifyEntries.Add(Json.Serialize(track)); /* FIXME */
			
			string url = string.Format(URL_MODIFY_ENTRIES, GetXtCookie());
			WebRequest request = MakeRequest(url, generateJson(modifyEntries));
			WebResponse response = request.GetResponse();
			using (var stream = response.GetResponseStream()) {
				var result = (JsonObject) Json.Unserialize(stream);
				if (result.ContainsKey("success"))
					return (bool) result["success"];
				else
					return false;
			}
		}
	}
}