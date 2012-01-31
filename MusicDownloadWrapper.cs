using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

using Banshee.Web;

namespace Banshee.GoogleMusic
{
	public class MusicDownloadWrapper : BaseHttpServer
	{
		private static readonly Regex TRACK_REGEX = new Regex("^/track/(.+)$", RegexOptions.Compiled);
		
		private Google.Music.Api api;
		
		public MusicDownloadWrapper (Google.Music.Api api) : base(new IPEndPoint(IPAddress.Loopback, 0), "music-download-wrapper")
		{
			this.api = api;
		}
		
		private void WriteRedirect (Socket client, string location)
		{
			if (!client.Connected)
				return;
			
			var data = new StringBuilder()
						.Append("HTTP/1.1 302 Redirect\n")
						.AppendFormat("Location: {0}\n", location)
					  	.Append("Connection: close\n\n")
						.ToString();
			
			using (var writer = new BinaryWriter(new NetworkStream(client))) {
				writer.Write(Encoding.UTF8.GetBytes(data));
			}

			client.Close();
		}
		
		private void WriteResponse (Socket client, HttpStatusCode code)
		{
			WriteResponse(client, code, "");
		}
		
		protected override void HandleValidRequest (Socket client, string[] split_request, string[] request_headers)
		{
			if (split_request == null || split_request.Length != 3 || split_request[0] != "GET") {
				WriteResponse(client, HttpStatusCode.BadRequest);
				return;
			}

			var requestUrl = split_request[1];
			if (TRACK_REGEX.IsMatch(requestUrl)) {
				var match = TRACK_REGEX.Match(requestUrl);
				var url = api.PlayTrack(match.Groups[1].Value);
				if (!string.IsNullOrEmpty(url))
					WriteRedirect(client, url);
				else
					WriteResponse(client, HttpStatusCode.InternalServerError);
			} else {
				WriteResponse(client, HttpStatusCode.NotFound);
			}
		}
		
		public string formTrackUrl(string trackId)
		{
			return string.Format("http://localhost:{0}/track/{1}", Port, trackId);
		}
	}
}