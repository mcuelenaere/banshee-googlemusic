using System;

using Banshee.Sources;
using Banshee.Collection;
using Banshee.ServiceStack;

using Hyena.Collections;

namespace Banshee.GoogleMusic
{
	public class MusicSource : Source, ITrackModelSource, IDisposable
	{
		private MemoryTrackListModel trackListModel = new MemoryTrackListModel();
		private Google.Music.Api api;
		private MusicDownloadWrapper downloadWrapper;
		
		public MusicSource () : base("Google Music", "Google Music", 30)
		{
			api = new Google.Music.Api();
			downloadWrapper = new MusicDownloadWrapper(api);
			downloadWrapper.Start();
			
			TypeUniqueId = "google-music";
			Properties.Set<Gdk.Pixbuf>("Icon.Pixbuf_16", Gdk.Pixbuf.LoadFromResource("google-music-favicon"));

			var win = new Gtk.Window("Google Music Login");
			var loginWidget = new LoginWidget();
			loginWidget.UserLoggedIn += (cookies) => {
				api.SetCookies(cookies);
				Reload();
				win.Destroy();
			};
			win.Add(loginWidget);
			win.ShowAll();
		}

		public void Dispose ()
		{
			downloadWrapper.Stop();
		}
		
		private TrackInfo createTrackInfo(Google.Music.Track track) {
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, new System.Globalization.GregorianCalendar(), DateTimeKind.Utc);
			return new TrackInfo() {
				AlbumArtist = track.albumArtist,
				AlbumTitle = track.title,
				ArtistName = track.artist,
				Bpm = track.beatsPerMinute,
				CanPlay = true,
				CanSaveToDatabase = false,
				Comment = track.comment,
				Composer = track.composer,
				DateAdded = epoch.AddTicks(track.creationDate*10),
				DiscCount = track.totalDiscs,
				DiscNumber = track.disc,
				Duration = TimeSpan.FromMilliseconds(track.durationMillis),
				Genre = track.genre,
				LastPlayed = epoch.AddTicks(track.lastPlayed*10),
				MediaAttributes = TrackMediaAttributes.AudioStream | TrackMediaAttributes.Music,
				MimeType = "audio/mp3",
				PlayCount = track.playCount,
				Rating = track.rating,
				TrackCount = track.totalTracks,
				TrackNumber = track.track,
				TrackTitle = track.title,
				Year = track.year,
				Uri = new Hyena.SafeUri(downloadWrapper.formTrackUrl(track.id)),
			};
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
		
        public void RemoveTracks (Selection selection) {
		}
		
        public void DeleteTracks (Selection selection) {
		}
		
		public override string PreferencesPageId {
			get {
				return "";
			}
		}
		
        public bool HasDependencies {
			get {
				return false;
			}
		}
		
		public override bool HasEditableTrackProperties {
			get {
				return true;
			}
		}
		
        public bool CanAddTracks {
			get {
				return false;
			}
		}
		
        public bool CanRemoveTracks {
			get {
				return false;
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