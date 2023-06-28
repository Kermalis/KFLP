using Kermalis.EndianBinaryIO;
using System.Collections.Generic;
using System.IO;

namespace Kermalis.FLP;

public sealed class FLArrangement
{
	internal const int NUM_PLAYLIST_TRACKS = 500;

	internal ushort Index;

	public string Name;
	public readonly List<FLPlaylistItem> PlaylistItems;
	public readonly List<FLPlaylistMarker> PlaylistMarkers;
	public readonly FLPlaylistTrack[] PlaylistTracks;

	public FLArrangement(string name)
	{
		Name = name;
		PlaylistItems = new List<FLPlaylistItem>();
		PlaylistMarkers = new List<FLPlaylistMarker>();

		PlaylistTracks = new FLPlaylistTrack[NUM_PLAYLIST_TRACKS];
		for (ushort i = 0; i < NUM_PLAYLIST_TRACKS; i++)
		{
			PlaylistTracks[i] = new FLPlaylistTrack(i);
		}
	}

	public void AddToPlaylist(FLPattern p, uint tick, uint duration, FLPlaylistTrack track)
	{
		PlaylistItems.Add(new FLPlaylistItem(tick, p, duration, track));
	}
	public void AddToPlaylist(FLAutomation a, uint tick, uint duration, FLPlaylistTrack track)
	{
		PlaylistItems.Add(new FLPlaylistItem(tick, a, duration, track));
	}
	public void AddTimeSigMarker(uint tick, byte num, byte denom)
	{
		PlaylistMarkers.Add(new FLPlaylistMarker(tick, num + "/" + denom, (num, denom)));
	}

	internal void ReadPlaylistItems(byte[] bytes, bool fl21)
	{
		using (var ms = new MemoryStream(bytes))
		{
			var r = new EndianBinaryReader(ms);

			int num = bytes.Length / (fl21 ? FLPlaylistItem.LEN_FL21 : FLPlaylistItem.LEN_FL20);
			PlaylistItems.Capacity = num;
			for (int i = 0; i < num; i++)
			{
				PlaylistItems.Add(new FLPlaylistItem(r, fl21));
			}
		}
	}
	internal void Write(EndianBinaryWriter w, FLVersionCompat verCom)
	{
		FLProjectWriter.Write16BitEvent(w, FLEvent.NewArrangement, Index);
		FLProjectWriter.WriteUTF16EventWithLength(w, FLEvent.ArrangementName, Name + '\0');
		FLProjectWriter.Write8BitEvent(w, FLEvent.Unk_36, 0);

		// Playlist Items. Must be in order of AbsoluteTick
		PlaylistItems.Sort((p1, p2) => p1.AbsoluteTick.CompareTo(p2.AbsoluteTick));

		w.WriteEnum(FLEvent.PlaylistItems);
		FLProjectWriter.WriteArrayEventLength(w, (uint)PlaylistItems.Count * FLPlaylistItem.LEN_FL20);
		foreach (FLPlaylistItem item in PlaylistItems)
		{
			item.Write(w);
		}

		// Playlist Markers
		foreach (FLPlaylistMarker mark in PlaylistMarkers)
		{
			mark.Write(w);
		}

		// Playlist Tracks
		foreach (FLPlaylistTrack track in PlaylistTracks)
		{
			track.Write(w, verCom);
		}
	}

	public override string ToString()
	{
		return Name;
	}
}
