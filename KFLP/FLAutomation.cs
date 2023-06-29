using Kermalis.EndianBinaryIO;
using System;
using System.Collections.Generic;

namespace Kermalis.FLP;

public sealed partial class FLAutomation
{
	public enum MyType : byte
	{
		Volume,
		Panpot,
		Pitch,
		MIDIProgram,
		Tempo,
	}

	private static ReadOnlySpan<byte> ChanPoly => new byte[9]
	{
		0x01, 0x00, 0x00, 0x00,
		0xF4, 0x01, // 500. TODO: Is this related to the number of playlist tracks?
		0x00, 0x00,
		0x00
	};

	internal ushort Index;

	public string Name;
	public FLColor3 Color;
	public readonly MyType Type;
	/// <summary>Only null for Tempo</summary>
	public readonly List<FLWriteChannel>? Targets;
	public FLAutomationData AutoData;
	public FLChannelFilter Filter;
	public int PitchBendOrTimeRange;

	internal FLAutomation(string name, MyType type, List<FLWriteChannel>? targets, FLChannelFilter filter)
	{
		Name = name;
		Color = GetDefaultColor(type);
		Type = type;
		Targets = targets;
		AutoData = new FLAutomationData();
		Filter = filter;
		PitchBendOrTimeRange = 2;
	}

	public static FLColor3 GetDefaultColor(MyType type)
	{
		switch (type)
		{
			case MyType.Volume: return new FLColor3(142, 96, 96);
			case MyType.Panpot: return new FLColor3(129, 142, 96);
			case MyType.Pitch: return new FLColor3(142, 96, 136);
			case MyType.MIDIProgram: return new FLColor3(109, 96, 142);
			case MyType.Tempo: return new FLColor3(142, 115, 96);
		}
		throw new ArgumentOutOfRangeException(nameof(type), type, null);
	}

	internal void Write(EndianBinaryWriter w, FLVersionCompat verCom, uint ppqn)
	{
		FLProjectWriter.Write16BitEvent(w, FLEvent.NewChannel, Index);
		FLProjectWriter.Write8BitEvent(w, FLEvent.ChannelType, (byte)FLChannelType.Automation);

		FLProjectWriter.WriteUTF16EventWithLength(w, FLEvent.DefPluginName, "\0");
		FLNewPlugin.WriteAutomation(w);
		FLProjectWriter.WriteUTF16EventWithLength(w, FLEvent.PluginName, Name + '\0');
		FLProjectWriter.Write32BitEvent(w, FLEvent.PluginIcon, 0);
		FLProjectWriter.Write32BitEvent(w, FLEvent.PluginColor, Color.GetFLValue());
		if (verCom == FLVersionCompat.V21_0_3__B3517)
		{
			// Always 1 with automation even if you use the theme's suggested default colors
			FLProjectWriter.Write8BitEvent(w, FLEvent.PluginIgnoresTheme, 1);
		}
		// No plugin params

		FLProjectWriter.Write8BitEvent(w, FLEvent.ChannelIsEnabled, 1);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.Delay, FLWriteChannel.Delay);
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
		FLBasicChannelParams.WriteAutomation(w);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChanOfsLevels, FLWriteChannel.ChanOfsLevels);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChanPoly, ChanPoly);
		FLChannelParams.WriteAutomation(w, PitchBendOrTimeRange);
		FLProjectWriter.Write32BitEvent(w, FLEvent.CutCutBy, 0);
		FLProjectWriter.Write32BitEvent(w, FLEvent.ChannelLayerFlags, 0);
		FLProjectWriter.Write32BitEvent(w, FLEvent.ChanFilterNum, Filter.Index);
		AutoData.Write(w, ppqn, Type);
		FLProjectWriter.Write8BitEvent(w, FLEvent.Unk_32, 0);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelTracking, FLWriteChannel.Tracking0);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelTracking, FLWriteChannel.Tracking1);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, FLWriteChannel.EnvelopeOther);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, FLWriteChannel.Envelope1);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, FLWriteChannel.EnvelopeOther);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, FLWriteChannel.EnvelopeOther);
		FLProjectWriter.WriteArrayEventWithLength(w, FLEvent.ChannelEnvelope, FLWriteChannel.EnvelopeOther);
		FLProjectWriter.Write32BitEvent(w, FLEvent.ChannelSampleFlags, 0b0011);
		FLProjectWriter.Write8BitEvent(w, FLEvent.ChannelLoopType, 0);
	}
	internal void WriteAutomationConnection(EndianBinaryWriter w)
	{
		if (Type == MyType.Tempo)
		{
			WriteAutomationConnection(w, 0x4000);
		}
		else
		{
			foreach (FLWriteChannel target in Targets!)
			{
				WriteAutomationConnection(w, target.Index);
			}
		}
	}
	private void WriteAutomationConnection(EndianBinaryWriter w, ushort target)
	{
		w.WriteEnum(FLEvent.AutomationConnection);
		FLProjectWriter.WriteArrayEventLength(w, 20);

		w.WriteUInt16(0);
		w.WriteUInt16(Index);
		w.WriteUInt32(0);
		w.WriteUInt16(MyTypeToConnectionType(Type));
		w.WriteUInt16(target);
		w.WriteUInt32(8);
		w.WriteUInt32(0x1D5);
	}
	private static ushort MyTypeToConnectionType(MyType t)
	{
		switch (t)
		{
			case MyType.Volume: return 0x0000;
			case MyType.Panpot: return 0x0001;
			case MyType.Pitch: return 0x0004;
			case MyType.Tempo: return 0x0005;
			case MyType.MIDIProgram: return 0x8000;
		}
		throw new ArgumentOutOfRangeException(nameof(t), t, null);
	}
}
