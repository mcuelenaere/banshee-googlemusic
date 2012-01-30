using System;

using Banshee.Sources;
using Banshee.Collection;
using Banshee.ServiceStack;

using Hyena.Collections;

namespace Banshee.GoogleMusic
{
	class GmUri : Hyena.SafeUri {
		public GmUri(string uri) : base(uri) {
		}
		
		public override string ToString ()
		{
			Console.WriteLine("Asking for ToString");
		 	return base.ToString();
		}
		
		public new string AbsolutePath {
			get {
				Console.WriteLine("Asking for AbsolutePath");
				return base.AbsolutePath;
			}
		}
		
		public new string AbsoluteUri {
			get {
				Console.WriteLine("Asking for AbsoluteUri");
				return base.AbsoluteUri;
			}
		}
	}
	
	public class MusicSource : Source, ITrackModelSource
	{
		private MemoryTrackListModel trackListModel = new MemoryTrackListModel();
		private Google.Music.Api api;
		
		public MusicSource () : base("Google Music", "Google Music", 30)
		{
			TypeUniqueId = "google-music";
			Properties.Set<Gdk.Pixbuf>("Icon.Pixbuf_16", Gdk.Pixbuf.LoadFromResource("google-music-favicon"));

			ServiceManager.PlayerEngine.TrackIntercept += (track) => {
				if (track != null && track.Uri.Scheme == "gmusic") {
					string trackId = track.Uri.AbsoluteUri.Substring("gmusic://".Length);
					string url = api.PlayTrack(trackId);
					track.Uri = new Hyena.SafeUri(url);
				}
				
				return false;
			};
			
			var win = new Gtk.Window("Google Music Login");
			var loginWidget = new LoginWidget();
			loginWidget.UserLoggedIn += (cookies) => {
				api = new Google.Music.Api(cookies);
				Reload();
				win.Destroy();
			};
			win.Add(loginWidget);
			win.ShowAll();
		}
		
		private TrackInfo createTrackInfo(Google.Music.Track track) {
			return new TrackInfo() {
				AlbumArtist = track.albumArtist,
				AlbumTitle = track.title,
				ArtistName = track.artist,
				Bpm = track.beatsPerMinute,
				CanPlay = true,
				CanSaveToDatabase = false,
				Comment = track.comment,
				Composer = track.composer,
				DateAdded = DateTime.FromFileTimeUtc(track.creationDate),
				DiscCount = track.totalDiscs,
				DiscNumber = track.disc,
				Duration = TimeSpan.FromMilliseconds(track.durationMillis),
				Genre = track.genre,
				LastPlayed = DateTime.FromFileTimeUtc(track.lastPlayed),
				MediaAttributes = TrackMediaAttributes.AudioStream | TrackMediaAttributes.Music,
				MimeType = "audio/mp3",
				PlayCount = track.playCount,
				Rating = track.rating,
				TrackCount = track.totalTracks,
				TrackNumber = track.track,
				TrackTitle = track.title,
				Year = track.year,
				Uri = new Hyena.SafeUri("gmusic://" + track.id),
			};
		}
		
		public override bool HasEditableTrackProperties {
			get {
				return true;
			}
		}
				
		public override int Count {
			get {
				return trackListModel.Count;
			}
		}
		
        public TrackListModel TrackModel {
			get {
				return trackListModel;
			}
		}

        public void Reload () {
			trackListModel.Clear();
			foreach (var track in api.GetTracks()) {
				trackListModel.Add(createTrackInfo(track));
			}
			
			OnUpdated();
		}
		
        public bool HasDependencies {
			get {
				return false;
			}
		}

        public void RemoveTracks (Selection selection) {
		}
		
        public void DeleteTracks (Selection selection) {
		}

        public bool CanAddTracks {
			get {
				return false;
			}
		}
		
        public bool CanRemoveTracks {
			get {
				return true;
			}
		}
		
        public bool CanDeleteTracks {
			get {
				return true;
			}
		}
		
        public bool ConfirmRemoveTracks {
			get {
				return true;
			}
		}

        public bool CanRepeat {
			get {
				return true;
			}
		}
		
        public bool CanShuffle {
			get {
				return true;
			}
		}

        public bool ShowBrowser {
			get {
				return true;
			}
		}
		
        public bool Indexable {
			get {
				return false;
			}
		}
	}
}