﻿using Kermalis.EndianBinaryIO;
using System.IO;

namespace Kermalis.FLP;

public sealed class FLPlaylistTrack
{
	public const float SIZE_MIN = 0f;
	public const float SIZE_DEFAULT = 1f;
	public const float SIZE_MAX = 25.9249992370605f;
	public static FLColor3 DefaultColor => new(72, 81, 86);

	internal readonly ushort Index;
	public ushort ID => (ushort)(Index + 1);

	public float Size;
	public bool GroupWithAbove;
	/// <summary>Only works if this track is the parent of the group</summary>
	public bool IsGroupCollapsed;
	public string? Name;
	public FLColor3 Color;
	public uint Icon;

	internal FLPlaylistTrack(ushort index)
	{
		Index = index;
		Color = DefaultColor;
		Size = SIZE_DEFAULT;
	}
	internal void Read(byte[] bytes)
	{
		using (var ms = new MemoryStream(bytes))
		{
			var r = new EndianBinaryReader(ms);

			r.ReadUInt16(); // ID

			Color.R = r.ReadByte();
			Color.G = r.ReadByte();
			Color.B = r.ReadByte();
			r.ReadByte(); // 0

			Icon = r.ReadUInt32();

			r.ReadByte(); // 1

			Size = r.ReadSingle();

			r.ReadInt32();

			r.ReadByte(); // 0
			r.ReadByte(); // 0
			r.ReadInt16(); // 0

			r.ReadByte(); // 0
			r.ReadByte(); // 0
			r.ReadInt16(); // 0

			r.ReadByte(); // 0
			r.ReadByte(); // 5
			r.ReadInt16(); // 0

			r.ReadByte(); // 0
			r.ReadBoolean(); // False
			r.ReadInt16(); // 0

			r.ReadByte(); // 0
			r.ReadBoolean(); // True
			r.ReadInt16(); // 0

			r.ReadByte(); // 0
			r.ReadByte(); // 0
			r.ReadInt16(); // 0

			r.ReadByte(); // 0
			GroupWithAbove = r.ReadBoolean();
			r.ReadInt16(); // 0

			r.ReadInt32();

			r.ReadInt32();
			r.ReadInt32();

			IsGroupCollapsed = !r.ReadBoolean();

			r.ReadInt32();
		}
	}

	internal void Write(EndianBinaryWriter w, FLVersionCompat verCom)
	{
		WriteNewPlaylistTrack(w);
		if (verCom == FLVersionCompat.V21_0_3__B3517)
		{
			byte val = (byte)(Color.Equals(DefaultColor) ? 0 : 1);
			FLProjectWriter.Write8BitEvent(w, FLEvent.PlaylistTrackIgnoresTheme, val);
		}
		if (Name is not null)
		{
			FLProjectWriter.WriteUTF16EventWithLength(w, FLEvent.PlaylistTrackName, Name + '\0');
		}
	}
	private void WriteNewPlaylistTrack(EndianBinaryWriter w)
	{
		w.WriteEnum(FLEvent.NewPlaylistTrack);
		FLProjectWriter.WriteArrayEventLength(w, 66);

		w.WriteUInt32(ID);

		w.WriteByte(Color.R);
		w.WriteByte(Color.G);
		w.WriteByte(Color.B);
		w.WriteByte(0);

		w.WriteUInt32(Icon);

		w.WriteByte(1);

		w.WriteSingle(Size);

		// The default height in pixels is 56
		// If I "Lock to this size", this becomes 0x38 (56) instead of -16 or -1
		// If I manually resize it, this becomes -56 and Size (above) changes
		// Even if I reset the size to 100%, this stays -56 instead of going back to the weird value
		w.WriteInt32(Index <= 0x20 ? -16 : -1); // TODO: Why?

		w.WriteByte(0);
		w.WriteByte(0); // Performance Motion
		w.WriteInt16(0);

		w.WriteByte(0);
		w.WriteByte(0); // Performance Press
		w.WriteInt16(0);

		w.WriteByte(0);
		w.WriteByte(5); // Performance Trigger Sync (4 beats)
		w.WriteInt16(0);

		w.WriteByte(0);
		w.WriteBoolean(false); // Performance Queue
		w.WriteInt16(0);

		w.WriteByte(0);
		w.WriteBoolean(true); // Performance Tolerant
		w.WriteInt16(0);

		w.WriteByte(0);
		w.WriteByte(0); // Performance Position Sync
		w.WriteInt16(0);

		w.WriteByte(0);
		w.WriteBoolean(GroupWithAbove);
		w.WriteInt16(0);

		w.WriteInt32(0); // Was 1 in "track mode - audio track" and 3 in "track mode - instrument track"

		w.WriteInt32(-1); // In audio track mode, it was the insert
		w.WriteInt32(-1); // In instrument track mode, it was the channelID

		w.WriteBoolean(!IsGroupCollapsed);

		w.WriteInt32(0); // Track Mode Instrument Track Options
	}

	public override string ToString()
	{
		return Name ?? $"#{ID}";
	}
}
