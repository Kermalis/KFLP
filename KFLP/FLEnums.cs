﻿using System;

namespace Kermalis.FLP;

public enum FLVersionCompat : byte
{
	V20_9_2__B2963,
	V21_0_3__B3517,
}

[Flags]
public enum FLPlaylistItemFlags : byte
{
	None = 0,
	Unk_1 = 1 << 1,
	Unk_2 = 1 << 2,
	Unk_3 = 1 << 3,
	Unk_4 = 1 << 4,
	Disabled = 1 << 5,
	Unk_6 = 1 << 6,
	Selected = 1 << 7,
}
public enum FLPlaylistMarkerType : byte
{
	None = 0,
	MarkerLoop = 1,
	MarkerSkip = 2,
	MarkerPause = 3,
	Loop = 4,
	Start = 5,
	// 6 and 7 deprecated?
	TimeSig = 8,
	StartRecording = 9,
	StopRecording = 10,
}
public enum FLAutomationPointCurveType : byte
{
	SingleCurve = 0,
	DoubleCurve = 1,
	Hold = 2,
	Stairs = 3,
	SmoothStairs = 4,
	Pulse = 5,
	Wave = 6,
	SingleCurve2 = 7,
	DoubleCurve2 = 8,
	HalfSine = 9,
	Smooth = 10,
	SingleCurve3 = 11,
	DoubleCurve3 = 12,
}

internal enum FLChannelType : byte
{
	Sampler = 0,
	/// <summary>Doesn't exist past FL12</summary>
	TS404 = 1,
	FruityWrapper = 2,
	Layer = 3,
	Audio = 4,
	Automation = 5,
}

[Flags]
internal enum FLFadeStereo : ushort
{
	None = 0,
	Unk_0 = 1 << 0,
	SampleReversed = 1 << 1,
	Unk_2 = 1 << 2,
	Unk_3 = 1 << 3,
	Unk_4 = 1 << 4,
	Unk_5 = 1 << 5,
	Unk_6 = 1 << 6,
	Unk_7 = 1 << 7,
	SampleReverseStereo = 1 << 8,
	Unk_9 = 1 << 9,
	Unk_10 = 1 << 10,
	Unk_11 = 1 << 11,
	Unk_12 = 1 << 12,
	Unk_13 = 1 << 13,
	Unk_14 = 1 << 14,
	Unk_15 = 1 << 15,
}

internal enum FLMixerParamsEvent : byte
{
	SlotState = 0x0,
	SlotVolume = 0x1,
	SlotDryWet = 0x2,
	Unk_4A = 0x4A,
	Unk_A4 = 0xA4,
	Unk_A5 = 0xA5,
	Unk_A6 = 0xA6,
	Unk_A7 = 0xA7,
	Unk_A8 = 0xA8,
	Unk_BE = 0xBE,
	Volume = 0xC0,
	Pan = 0xC1,
	StereoSeparation = 0xC2,
	LowLevel = 0xD0,
	BandLevel = 0xD1,
	HighLevel = 0xD2,
	LowFreq = 0xD8,
	BandFreq = 0xD9,
	HighFreq = 0xDA,
	LowWidth = 0xE0,
	BandWidth = 0xE1,
	HighWidth = 0xE2,
}

[Flags]
internal enum InsertFlags : ushort
{
	None = 0,
	ReversePolarity = 1 << 0,
	SwapChannels = 1 << 1,
	Unk_2 = 1 << 2,
	Unmuted = 1 << 3,
	DisableThreaded = 1 << 4,
	Unk_5 = 1 << 5,
	DockMiddle = 1 << 6,
	DockRight = 1 << 7,
	Unk_8 = 1 << 8,
	Unk_9 = 1 << 9,
	Separator = 1 << 10,
	Lock = 1 << 11,
	Solo = 1 << 12,
	Unk_13 = 1 << 13,
	Unk_14 = 1 << 14,
	Unk_15 = 1 << 15,
}

internal enum ArpDirection : byte
{
	Off = 0,
	Up = 1,
	Down = 2,
	UpDownBounce = 3,
	UpDownSticky = 4,
	Random = 5,
}