using Kermalis.EndianBinaryIO;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kermalis.FLP;

public sealed class FLAutomationData
{
	public struct Point
	{
		internal const int LEN = 24;
		// For the last point, its hex value is 0xFFFFFFFF00000001, which is not a normal NaN.
		// Normal NaN is 0x7FF8000000000000
		private const ulong LAST_POINT_DELTA = 0xFFFFFFFF00000001;

		public uint AbsoluteTicks;
		/// <summary>Tension from previous point [-1, 1]</summary>
		public float Tension;
		// 0x00000000 for first point?
		// 0x01000000 for "single curve" edited..?
		// 0x02000000 for "single curve"?
		// 0x02000001 for "double curve"?
		// 0x00000002 for "hold".
		// 0x00000003 for "stairs"?
		// 0x00000004 for "smooth stairs"?
		// 0x00000005 for "pulse"?
		// 0x01000005 for "pulse" edited..?
		// 0x00000006 for "wave"?
		// 0xff000006 for -1/-0.01 tension "wave"
		// 0x01000006 for +0.01/+0.91 tension "wave"
		// 0x02000006 for +0.99/+1 tension "wave"
		// 0x01000007 for "single curve 2" edited..?
		// 0x02000007 for "single curve 2"?
		// 0x02000008 for "double curve 2"?
		// 0x02000009 for "half sine"?
		// 0x0000000a for "smooth"
		// 0x0200000B for "single curve 3"?
		// 0x0200000C for "double curve 3"?
		private readonly uint _curveType = 2;
		public double Value;

		public readonly FLAutomationPointCurveType CurveType => (FLAutomationPointCurveType)_curveType;

		internal Point(EndianBinaryReader r, uint ticks, out double nextPointDelta)
		{
			AbsoluteTicks = ticks;
			Value = r.ReadDouble();
			Tension = r.ReadSingle();
			_curveType = r.ReadUInt32();

			// Next point deltatime (in quarters of a bar)
			nextPointDelta = r.ReadDouble();
		}

		internal readonly void Write(EndianBinaryWriter w, uint ppqn, bool isFirst, bool isLast, uint nextPointAbsoluteTicks)
		{
			w.WriteDouble(Value);
			// Tension and curve type are in buggy states when you convert events to an automation clip. Not recreating that behavior
			w.WriteSingle(Tension);
			w.WriteUInt32(isFirst ? 0u : _curveType);

			// Delta ticks in quarter bars
			if (isLast)
			{
				w.WriteUInt64(LAST_POINT_DELTA);
			}
			else
			{
				uint deltaTicks = nextPointAbsoluteTicks - AbsoluteTicks;
				w.WriteDouble(deltaTicks / (double)ppqn);
			}
		}
	}

	private const int TWO_POINT_LEN = 181;
	private const int NO_POINT_LEN = TWO_POINT_LEN - (2 * Point.LEN);

	public readonly List<Point> Points;

	internal FLAutomationData()
	{
		Points = new List<Point>();
	}
	internal FLAutomationData(byte[] data, ushort ppqn)
	{
		using (MemoryStream ms = new(data))
		{
			var r = new EndianBinaryReader(ms);

			r.ReadUInt32(); // 1
			r.ReadUInt32(); // 0x40
			r.ReadBoolean(); // GraphHasPositiveAndNegative
			r.ReadUInt32(); // 4
			r.ReadUInt32(); // 3

			int numPoints = r.ReadInt32();
			Points = new List<Point>(numPoints);

			int verify = (data.Length - NO_POINT_LEN) / Point.LEN;
			if (numPoints != verify)
			{
				throw new InvalidDataException("AutomationData length mismatch");
			}

			r.ReadUInt32(); // 0
			r.ReadUInt32(); // 0

			uint absoluteTicks = 0;
			for (int p = 0; p < numPoints; p++)
			{
				Points.Add(new Point(r, absoluteTicks, out double nextPointDelta));
				uint deltaTicks = (uint)(nextPointDelta * ppqn);
				absoluteTicks += deltaTicks;
			}

			r.ReadUInt32(); // -1
			r.ReadUInt32(); // -1
			r.ReadUInt32(); // -1
			r.ReadUInt32(); // 0x80
			r.ReadUInt32(); // 0x80
			r.ReadUInt32(); // 0
			r.ReadUInt32(); // 0x80
			r.ReadUInt32(); // 5
			r.ReadUInt32(); // 3
			r.ReadUInt32(); // 1
			r.ReadUInt32(); // 0
			r.ReadUInt32(); // 0
			r.ReadDouble(); // 1d
			r.ReadUInt32(); // 0
			r.ReadUInt32(); // 0
			r.ReadUInt32(); // 1
			r.ReadUInt32(); // 0
			r.ReadUInt32(); // -1
			r.ReadUInt32(); // -1
			r.ReadUInt32(); // -1
			r.ReadUInt32(); // 0xB2FB (45_819)
			r.ReadUInt32(); // 0
			r.ReadUInt32(); // 0
			r.ReadUInt32(); // 0
			r.ReadUInt32(); // 0
		}
	}

	public void AddPoint(uint ticks, double value)
	{
		Points.Add(new Point
		{
			AbsoluteTicks = ticks,
			Value = value,
		});
	}
	public void AddTempoPoint(uint ticks, decimal bpm)
	{
		// Default min/max (10%/33%): 60 is 0.0d, 120 is 0.5d, 140 is 0.6666666865348816d, 180 is 1.0d
		// Min/Max 0%/100%: 0.0d is 10, 0.5d is 266, 1.0d is 522

		AddPoint(ticks, (double)FLUtils.LerpUnclamped(10, 522, 0, 1, bpm));
	}
	public void PadPoints(uint targetTicks, double defaultValue)
	{
		if (Points.Count == 0)
		{
			throw new Exception();
		}

		// Make sure there's a point at 0
		Point firstPoint = Points[0];
		if (firstPoint.AbsoluteTicks != 0)
		{
			Points.Insert(0, new Point
			{
				AbsoluteTicks = 0,
				Value = defaultValue,
			});
		}

		// Make sure there's a point at targetTicks
		Point lastPoint = Points[Points.Count - 1];
		if (lastPoint.AbsoluteTicks != targetTicks)
		{
			AddPoint(targetTicks, lastPoint.Value);
		}
	}
	public void PadTempoPoints(uint targetTicks, double defaultTempo)
	{
		PadPoints(targetTicks, FLUtils.LerpUnclamped(10, 522, 0, 1, defaultTempo));
	}

	private static bool GraphHasPositiveAndNegative(FLAutomation.MyType t)
	{
		switch (t)
		{
			case FLAutomation.MyType.Volume:
			case FLAutomation.MyType.MIDIProgram:
			case FLAutomation.MyType.Tempo:
				return false;
			case FLAutomation.MyType.Panpot:
			case FLAutomation.MyType.Pitch:
				return true;
		}
		throw new ArgumentOutOfRangeException(nameof(t), t, null);
	}
	internal void Write(EndianBinaryWriter w, uint ppqn, FLAutomation.MyType type)
	{
		w.WriteEnum(FLEvent.AutomationData);

		uint numPoints = (uint)Points.Count;
		FLProjectWriter.WriteArrayEventLength(w, NO_POINT_LEN + (numPoints * Point.LEN));

		w.WriteUInt32(1);
		w.WriteUInt32(0x40);
		w.WriteBoolean(GraphHasPositiveAndNegative(type));
		w.WriteUInt32(4);
		w.WriteUInt32(3);
		w.WriteUInt32(numPoints);
		w.WriteUInt32(0);
		w.WriteUInt32(0);

		for (int i = 0; i < numPoints; i++)
		{
			bool isLast = i == numPoints - 1;
			Points[i].Write(w, ppqn, i == 0, isLast, isLast ? 0 : Points[i + 1].AbsoluteTicks);
		}

		w.WriteUInt32(uint.MaxValue);
		w.WriteUInt32(uint.MaxValue);
		w.WriteUInt32(uint.MaxValue);
		w.WriteUInt32(0x80);
		w.WriteUInt32(0x80);
		w.WriteUInt32(0);
		w.WriteUInt32(0x80);
		w.WriteUInt32(5);
		w.WriteUInt32(3);
		w.WriteUInt32(1);
		w.WriteUInt32(0);
		w.WriteUInt32(0);
		w.WriteDouble(1d);
		w.WriteUInt32(0);
		w.WriteUInt32(0);
		w.WriteUInt32(1);
		w.WriteUInt32(0);
		w.WriteUInt32(uint.MaxValue);
		w.WriteUInt32(uint.MaxValue);
		w.WriteUInt32(uint.MaxValue);
		w.WriteUInt32(0xB2FB);
		w.WriteUInt32(0);
		w.WriteUInt32(0);
		w.WriteUInt32(0);
		w.WriteUInt32(0);
	}
}
