﻿using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.FLP;

public sealed class FLWriteChannel
{
	/// <summary>Found in "Miscellaneous functions" of a channel. Automation channels have it too, despite that not being accessible in the GUI</summary>
	internal static ReadOnlySpan<byte> Delay => new byte[20]
	{
		0x00, 0x00, // 0-1: EchoFeed
		0x00, 0x00, 0x00, 0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		0x04, // 12: Echoes default=(4)
		0x00, 0x00, 0x00,
		0x90, 0x00, // 16-17: EchoTime default=(0x90 = 144 => 3:00)
		0x00, 0x00
	};
	internal static ReadOnlySpan<byte> ChanOfsLevels => new byte[20]
	{
		0x00, 0x00, 0x00, 0x00, // 0
		0x00, 0x32, 0x00, 0x00, // 12_800
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00
	};
	private static ReadOnlySpan<byte> ChanPoly => new byte[9]
	{
		0x00, 0x00, 0x00, 0x00,
		0xF4, 0x01, // 500
		0x00, 0x00,
		0x00
	};
	// 100
	internal static ReadOnlySpan<byte> Tracking0 => new byte[16] { 0x64, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
	// 60
	internal static ReadOnlySpan<byte> Tracking1 => new byte[16] { 0x3C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

	internal static ReadOnlySpan<byte> EnvelopeOther => new byte[68]
	{
		0x00,

		0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x64, 0x00, 0x00, 0x00, // 100
		0x20, 0x4E, 0x00, 0x00, // 20_000
		0x20, 0x4E, 0x00, 0x00, // 20_000
		0x30, 0x75, 0x00, 0x00, // 30_000
		0x32, 0x00, 0x00, 0x00, // 50
		0x20, 0x4E, 0x00, 0x00, // 20_000
		0x00, 0x00, 0x00, 0x00,
		0x64, 0x00, 0x00, 0x00, // 100
		0x20, 0x4E, 0x00, 0x00, // 20_000
		0x00, 0x00, 0x00, 0x00,
		0xB6, 0x80, 0x00, 0x00, // 32_950
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

		0x00, 0x00, 0x00, 0x00
	};
	internal static ReadOnlySpan<byte> Envelope1 => new byte[68]
	{
		0x04,

		0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x64, 0x00, 0x00, 0x00,
		0x20, 0x4E, 0x00, 0x00,
		0x20, 0x4E, 0x00, 0x00,
		0x30, 0x75, 0x00, 0x00,
		0x32, 0x00, 0x00, 0x00,
		0x20, 0x4E, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x64, 0x00, 0x00, 0x00,
		0x20, 0x4E, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0xB6, 0x80, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,

		0x9B, 0xFF, 0xFF, 0xFF // -101
	};

	public static FLColor3 DefaultColor => new(92, 101, 106);
	public static FLColor3 MIDIOutDefaultColor => new(96, 114, 115);

	internal ushort Index;

	public string Name;
	public FLColor3 Color;
	public byte MIDIChannel;
	public byte MIDIBank;
	public byte MIDIProgram;
	public FLChannelFilter Filter;
	public uint PanKnob;
	public uint VolKnob;
	/// <summary>In cents</summary>
	public int PitchKnob;
	public int PitchBendRange;

	internal FLWriteChannel(string name, byte midiChan, byte midiBank, byte midiProgram, FLChannelFilter filter)
	{
		Name = name;
		Color = MIDIOutDefaultColor;
		MIDIChannel = midiChan;
		MIDIBank = midiBank;
		MIDIProgram = midiProgram;
		Filter = filter;
		PanKnob = FLBasicChannelParams.DEFAULT_PAN;
		VolKnob = FLBasicChannelParams.DEFAULT_VOL;
		PitchKnob = 0;
		PitchBendRange = 2;
	}

	internal void Write(EndianBinaryWriter w, FLVersionCompat verCom)
	{
		FLProjectWriter.Write16BitEvent(w, FLEvent.NewChannel, Index);
		FLProjectWriter.Write8BitEvent(w, FLEvent.ChannelType, (byte)FLChannelType.FruityWrapper);

		FLProjectWriter.WriteUTF16EventWithLength(w, FLEvent.DefPluginName, "MIDI Out\0");
		FLNewPlugin.WriteMIDIOut(w);
		FLProjectWriter.WriteUTF16EventWithLength(w, FLEvent.PluginName, Name + '\0');
		FLProjectWriter.Write32BitEvent(w, FLEvent.PluginIcon, 0);
		FLProjectWriter.Write32BitEvent(w, FLEvent.PluginColor, Color.GetFLValue());
		if (verCom == FLVersionCompat.V21_0_3__B3517)
		{
			// Always 1 with MIDIOut even if you use the theme's suggested default colors
			// Sampler is 0 though if you use suggested
			FLProjectWriter.Write8BitEvent(w, FLEvent.PluginIgnoresTheme, 1);
		}
		FLPluginParams.WriteMIDIOut(w, MIDIChannel, MIDIBank, MIDIProgram);

		FLProjectWriter.Write8BitEvent(w, FLEvent.ChannelIsEnabled, 1);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.Delay, Delay);
		FLProjectWriter.Write32BitEvent(w, FLEvent.DelayReso, 0x800_080);
		FLProjectWriter.Write32BitEvent(w, FLEvent.Reverb, 0x10_000);
		FLProjectWriter.Write16BitEvent(w, FLEvent.ShiftDelay, 0);
		FLProjectWriter.Write16BitEvent(w, FLEvent.SwingMix, 0x80);
		FLProjectWriter.Write16BitEvent(w, FLEvent.FX, 0x80);
		FLProjectWriter.Write16BitEvent(w, FLEvent.FX3, 0x100);
		FLProjectWriter.Write16BitEvent(w, FLEvent.CutOff, 0x400);
		FLProjectWriter.Write16BitEvent(w, FLEvent.Resonance, 0);
		FLProjectWriter.Write16BitEvent(w, FLEvent.PreAmp, 0);
		FLProjectWriter.Write16BitEvent(w, FLEvent.Decay, 0);
		FLProjectWriter.Write16BitEvent(w, FLEvent.Attack, 0);
		FLProjectWriter.Write16BitEvent(w, FLEvent.StDel, 0x800);
		FLProjectWriter.Write32BitEvent(w, FLEvent.FXSine, 0x800_000);
		FLProjectWriter.Write16BitEvent(w, FLEvent.Fade_Stereo, (ushort)FLFadeStereo.None);
		FLProjectWriter.Write8BitEvent(w, FLEvent.TargetFXTrack, 0);
		FLBasicChannelParams.WriteChannel(w, PanKnob, VolKnob, PitchKnob);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChanOfsLevels, ChanOfsLevels);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChanPoly, ChanPoly);
		FLChannelParams.WriteMIDIOut(w, Index, PitchBendRange);
		FLProjectWriter.Write32BitEvent(w, FLEvent.CutCutBy, (uint)(Index + 1) * 0x10_001u);
		FLProjectWriter.Write32BitEvent(w, FLEvent.ChannelLayerFlags, 0);
		FLProjectWriter.Write32BitEvent(w, FLEvent.ChanFilterNum, Filter.Index);
		//
		FLProjectWriter.Write8BitEvent(w, FLEvent.Unk_32, 0);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelTracking, Tracking0);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelTracking, Tracking1);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, EnvelopeOther);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, Envelope1);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, EnvelopeOther);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, EnvelopeOther);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, EnvelopeOther);
		FLProjectWriter.Write32BitEvent(w, FLEvent.ChannelSampleFlags, 0b1010);
		FLProjectWriter.Write8BitEvent(w, FLEvent.ChannelLoopType, 0);
	}
}
