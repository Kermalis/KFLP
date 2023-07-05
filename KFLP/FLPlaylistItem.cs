using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.FLP;

public sealed class FLPlaylistItem
{
	internal const int LEN_FL20 = 32;
	internal const int LEN_FL21 = 60;

	public uint AbsoluteTick;
	public uint DurationTicks;
	public FLPattern? Pattern;
	public FLAutomation? WriteChannel; // TODO: Support audio clip writing
	/// <summary>Used for Automation or AudioClips</summary>
	public FLReadChannel? ReadChannel;
	/// <summary><see cref="uint.MaxValue"/> if unset (for patterns)</summary>
	public uint StartTicks;
	/// <inheritdoc cref="StartTicks"/>
	public uint EndTicksExclusive;
	public FLPlaylistTrack PlaylistTrack;
	public FLPlaylistItemFlags Flags;

	// TODO: Can make a struct that's used only while loading so all of the items don't contain this data. Same for other objects
	private readonly ushort _readPatternID;
	private readonly ushort _readChannelIndex;
	private readonly ushort _readPlaylistTrackID;

	/// <summary>-1 if unset. For AudioClips, this is in milliseconds. For Automations, this is in Quarters</summary>
	public float Start_Channel
	{
		get => BitConverter.UInt32BitsToSingle(StartTicks);
		set => StartTicks = BitConverter.SingleToUInt32Bits(value);
	}
	/// <inheritdoc cref="Start_Channel"/>
	public float EndExclusive_Channel
	{
		get => BitConverter.UInt32BitsToSingle(EndTicksExclusive);
		set => EndTicksExclusive = BitConverter.SingleToUInt32Bits(value);
	}

	public FLPlaylistItem(uint tick, FLPattern pattern, uint duration, FLPlaylistTrack track)
	{
		AbsoluteTick = tick;
		Pattern = pattern;
		DurationTicks = duration;
		PlaylistTrack = track;
		StartTicks = uint.MaxValue;
		EndTicksExclusive = uint.MaxValue;
	}
	public FLPlaylistItem(uint tick, FLAutomation chan, uint duration, FLPlaylistTrack track)
	{
		AbsoluteTick = tick;
		WriteChannel = chan;
		DurationTicks = duration;
		PlaylistTrack = track;
		Start_Channel = -1f;
		EndExclusive_Channel = -1f;
	}
	internal FLPlaylistItem(EndianBinaryReader r, bool fl21)
	{
		AbsoluteTick = r.ReadUInt32();

		r.ReadUInt16(); // 0x5000
		ushort u = r.ReadUInt16();
		if (u > 0x5000)
		{
			_readPatternID = (ushort)(u - 0x5000);
			_readChannelIndex = ushort.MaxValue;
		}
		else
		{
			_readPatternID = ushort.MaxValue;
			_readChannelIndex = u;
		}

		DurationTicks = r.ReadUInt32();

		_readPlaylistTrackID = (ushort)(FLArrangement.NUM_PLAYLIST_TRACKS - r.ReadUInt16());
		PlaylistTrack = null!;
		r.ReadUInt16(); // 0

		r.ReadByte(); // 120
		r.ReadByte(); // 0
		r.ReadByte(); // 64
		Flags = r.ReadEnum<FLPlaylistItemFlags>();

		r.ReadByte(); // 64
		r.ReadByte(); // 100
		r.ReadUInt16(); // 0x8080

		StartTicks = r.ReadUInt32();
		EndTicksExclusive = r.ReadUInt32();

		// FL21: AudioClip fade
		if (fl21)
		{
			r.ReadUInt32(); // 3
			r.ReadSingle(); // [FadeIn MS]
			r.ReadSingle(); // [FadeIn Tension]
			r.ReadSingle(); // [FadeOut MS]
			r.ReadSingle(); // [FadeOut Tension]
			r.ReadSingle(); // [Gain Linear]
			r.ReadByte(); // 0 = Unset | 1 = FadeIn manual | 2 = FadeOut manual | 3 = FadeIn/FadeOut manual
			r.ReadByte(); // 0 = InSmooth/OutSmooth | 1 = InSingle/OutSmooth | 0x10 = InSmooth/OutSingle | 0x11 InSingle/OutSingle
			r.ReadUInt16(); // 0

			// Tension values
			// -2,672,339.25         = -100%
			//       -516.846741     =  -50%
			//          0            =    0%
			//          0.0186503213 =    1%
			//          0.135149777  =    5%
			//          0.4529542    =   10%
			//          0.9958846    =   14%
			//          1            =   14.02%
			//          1.20027053   =   15%
			//          2.95758343   =   20%
			//          7.089901     =   25%
			//         16.8070354    =   30%
			//         39.6568565    =   35%
			//         93.38817      =   40%
			//        219.737213     =   45%
			//        516.846741     =   50%
			//      1,215.49915      =   55%
			//      2,858.37866      =   60%
			//      6,721.60645      =   65%
			//     15,805.9785       =   70%
			//     37,167.8555       =   75%
			//     87,400.27         =   80%
			//    205,521.688        =   85%
			//    483,283.9          =   90%
			//  1,136,441.25         =   95%
			//  2,252,281.5          =   99%
			//  2,672,339.25         =  100%
		}
	}

	internal void LoadObjects(FLProjectReader r, FLArrangement arr)
	{
		if (_readPatternID != ushort.MaxValue)
		{
			Pattern = r.Patterns.Find(p => p.ID == _readPatternID)!;
		}
		if (_readChannelIndex != ushort.MaxValue)
		{
			ReadChannel = r.Channels.Find(a => a.Index == _readChannelIndex);
		}
		PlaylistTrack = Array.Find(arr.PlaylistTracks, t => t.ID == _readPlaylistTrackID)!;
	}

	internal void Write(EndianBinaryWriter w, FLVersionCompat vercom)
	{
		w.WriteUInt32(AbsoluteTick);

		w.WriteUInt16(0x5000);
		if (WriteChannel is not null)
		{
			w.WriteUInt16(WriteChannel.Index);
		}
		else if (Pattern is not null)
		{
			w.WriteUInt16((ushort)(0x5000 + Pattern.ID));
		}
		else
		{
			throw new InvalidOperationException($"{nameof(WriteChannel)} and {nameof(Pattern)} were null");
		}

		w.WriteUInt32(DurationTicks);

		w.WriteUInt16((ushort)(FLArrangement.NUM_PLAYLIST_TRACKS - PlaylistTrack.ID));
		w.WriteUInt16(0);

		w.WriteByte(0x78); // 120
		w.WriteByte(0);
		w.WriteByte(0x40); // 64
		w.WriteEnum(Flags);

		w.WriteByte(0x40); // 64
		w.WriteByte(0x64); // 100
		w.WriteUInt16(0x8080);

		if (WriteChannel is not null)
		{
			w.WriteSingle(Start_Channel);
			w.WriteSingle(EndExclusive_Channel);
		}
		else
		{
			w.WriteUInt32(StartTicks);
			w.WriteUInt32(EndTicksExclusive);
		}

		// FL21: AudioClip fade
		if (vercom >= FLVersionCompat.V21_0_3__B3517)
		{
			w.WriteUInt32(3);
			w.WriteSingle(0f);
			w.WriteSingle(0f);
			w.WriteSingle(0f);
			w.WriteSingle(0f);
			w.WriteSingle(1f);
			w.WriteByte(0);
			w.WriteByte(0);
			w.WriteUInt16(0);
		}
	}
}
