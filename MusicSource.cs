using System;
using System.Collections.Generic;

using Banshee.Sources;
using Banshee.Collection;
using Banshee.ServiceStack;

using Hyena.Collections;

namespace Banshee.GoogleMusic
{
	public class MusicSource : PrimarySource, IDisposable
	{
		const int sort_order = 190;

		private Google.Music.Api api;
		private MusicDownloadWrapper downloadWrapper;

		public MusicSource () : base("Google Music", "Google Music", "google-music", sort_order)
		{
			api = new Google.Music.Api();
			downloadWrapper = new MusicDownloadWrapper(api);
			downloadWrapper.Start();
			
			Properties.Set<Gdk.Pixbuf>("Icon.Pixbuf_16", Gdk.Pixbuf.LoadFromResource("google-music-favicon"));

			AfterInitialized();

			var win = new Gtk.Window("Google Music Login");
			var loginWidget = new LoginWidget();
			loginWidget.UserLoggedIn += (cookies) => {
				api.SetCookies(cookies);
				AsyncUserJob.Create(() => {
					Refetch();
				}, "Fetching playlist");
				
				win.Destroy();
			};
			win.Add(loginWidget);
			win.ShowAll();
		}

		~MusicSource()
		{
			Dispose ();
		}

		public override void Dispose ()
		{
			downloadWrapper.Stop();
			base.Dispose();
		}

		public override bool CanDeleteTracks {
			get { return false; }
		}

		public override bool CanAddTracks {
			get { return false; }
		}

		private void Refetch ()
		{
			PurgeTracks();
			OnTracksRemoved();

			int counter = 0;
			foreach (var track in api.GetTracks()) {
				AddTrack(track);
				if (counter++ > 100) {
					OnTracksAdded(); // update GUI
					counter = 0;
				}
			}
			
			OnTracksAdded();
		}

		private void AddTrack(Google.Music.Track track)
		{
			var track_info = new MusicTrack(track, downloadWrapper.formTrackUrl(track.id), this);
			track_info.Save (false);
		}
	}
}