using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.FLP;

public sealed class FLPlaylistMarker
{
	public FLPlaylistMarkerType Type { get; private set; }
	public uint AbsoluteTicks { get; private set; }
	/// <summary>Empty uses default name "Marker #1" for example</summary>
	public string Name;
	public (byte num, byte denom)? TimeSig { get; internal set; }

	public FLPlaylistMarker(string name)
	{
		Name = name;
	}
	internal FLPlaylistMarker(uint val)
	{
		Type = (FLPlaylistMarkerType)(val >> 24);
		AbsoluteTicks = val & 0xFFFFFF;
		Name = string.Empty;
	}

	public void SetAbsoluteTicks(uint absoluteTicks)
	{
		if (absoluteTicks > 0xFFFFFF)
		{
			throw new ArgumentOutOfRangeException(nameof(absoluteTicks), absoluteTicks, null);
		}
		AbsoluteTicks = absoluteTicks;
	}

	public void SetType(FLPlaylistMarkerType type)
	{
		switch (type)
		{
			case FLPlaylistMarkerType.None:
			case FLPlaylistMarkerType.MarkerLoop:
			case FLPlaylistMarkerType.MarkerSkip:
			case FLPlaylistMarkerType.MarkerPause:
			case FLPlaylistMarkerType.Loop:
			case FLPlaylistMarkerType.Start:
			case FLPlaylistMarkerType.StartRecording:
			case FLPlaylistMarkerType.StopRecording:
			{
				Type = type;
				TimeSig = null;
				return;
			}
		}
		throw new ArgumentOutOfRangeException(nameof(type), type, null);
	}
	public void SetType_TimeSig(byte num, byte denom)
	{
		Type = FLPlaylistMarkerType.TimeSig;
		TimeSig = (num, denom);
	}

	internal void Write(EndianBinaryWriter w)
	{
		uint typeUint = (uint)Type << 24;
		FLProjectWriter.Write32BitEvent(w, FLEvent.NewTimeMarker, AbsoluteTicks | typeUint);

		if (TimeSig is not null)
		{
			(byte num, byte denom) = TimeSig.Value;
			FLProjectWriter.Write8BitEvent(w, FLEvent.TimeSigMarkerNumerator, num);
			FLProjectWriter.Write8BitEvent(w, FLEvent.TimeSigMarkerDenominator, denom);
		}
		FLProjectWriter.WriteUTF16EventWithLength(w, FLEvent.TimeMarkerName, Name + '\0');
	}

	public override string ToString()
	{
		return string.Concat(Type.ToString(), ": ", Name);
	}
}
