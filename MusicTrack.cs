using System;

using Banshee.Sources;
using Banshee.Collection;
using Banshee.Collection.Database;

namespace Banshee.GoogleMusic
{
	public class MusicTrack : DatabaseTrackInfo
	{
		private static long id = 0;

		public MusicTrack (Google.Music.Track track, string url, PrimarySource source) : base()
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, new System.Globalization.GregorianCalendar(), DateTimeKind.Utc);

			AlbumArtist = track.albumArtist;
			AlbumArtistSort = track.albumArtistNorm;
			ArtistName = track.artist;
			ArtistNameSort = track.artistNorm;
			AlbumTitle = track.album;
			AlbumTitleSort = track.albumNorm;
			Bpm = track.beatsPerMinute;
			CanPlay = true;
			CanSaveToDatabase = false;
			Comment = track.comment;
			Composer = track.composer;
			DateAdded = epoch.AddTicks(track.creationDate*10);
			DiscCount = track.totalDiscs;
			DiscNumber = track.disc;
			Duration = TimeSpan.FromMilliseconds(track.durationMillis);
			Genre = track.genre;
			LastPlayed = epoch.AddTicks(track.lastPlayed*10);
			MediaAttributes = TrackMediaAttributes.AudioStream | TrackMediaAttributes.Music;
			MimeType = "audio/mp3";
			PlayCount = track.playCount;
			Rating = track.rating;
			TrackCount = track.totalTracks;
			TrackNumber = track.track;
			TrackTitle = track.title;
			TrackTitleSort = track.titleNorm;
			Year = track.year;
			Uri = new Hyena.SafeUri(url);

			ExternalId = ++id;
			PrimarySource = source;
		}
	}
}

