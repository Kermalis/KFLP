using Kermalis.EndianBinaryIO;
using System;

namespace Kermalis.FLP;

public readonly struct FLProjectTime
{
	private static DateTime BaseDate => new(1899, 12, 30);

	public readonly DateTime Creation;
	public readonly TimeSpan TimeSpent;

	public FLProjectTime(DateTime creation, TimeSpan timeSpent)
	{
		Creation = creation;
		TimeSpent = timeSpent;
	}
	internal FLProjectTime(byte[] bytes)
	{
		double startOffset = EndianBinaryPrimitives.ReadDouble(bytes, Endianness.LittleEndian);
		double daysWorked = EndianBinaryPrimitives.ReadDouble(bytes.AsSpan(8), Endianness.LittleEndian);

		Creation = BaseDate.AddDays(startOffset);
		TimeSpent = TimeSpan.FromDays(daysWorked);
	}

	internal void Write(EndianBinaryWriter w)
	{
		w.WriteEnum(FLEvent.ProjectTime);
		FLProjectWriter.WriteArrayEventLength(w, 16);

		w.WriteDouble((Creation - BaseDate).TotalDays);
		w.WriteDouble(TimeSpent.TotalDays);
	}

	public override string ToString()
	{
		return string.Format("{{ Created: {0}, TimeSpent: {1} }}", Creation, TimeSpent);
	}
}
