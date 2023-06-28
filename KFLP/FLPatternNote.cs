using Kermalis.EndianBinaryIO;

namespace Kermalis.FLP;

public struct FLPatternNote
{
	internal const int LEN = 24;

	public uint AbsoluteTick;
	public bool Slide;
	public FLReadChannel ReadChannel;
	public FLWriteChannel WriteChannel;
	/// <summary>Infinite => 0</summary>
	public uint DurationTicks = 48;
	public byte Key;
	/// <summary>-1200 => 000, 0 => 120, +1200 => 240</summary>
	public byte Pitch = 120;
	/// <summary>0% => 0x00, 50% => 0x40, 100% => 0x80</summary>
	public byte Release = 0x40;
	/// <summary>0 through F</summary>
	public byte Color;
	public bool Portamento;
	/// <summary>100% left => 0x00, center => 0x40, 100% right => 0x80</summary>
	public byte Panpot = 0x40;
	/// <summary>0% => 0x00, "80%" => 0x64, 100% => 0x80</summary>
	public byte Velocity = 0x64;
	/// <summary>-100 => 0x00, 0 => 0x80, +100 => 0xFF</summary>
	public byte ModX = 0x80;
	/// <summary>-100 => 0x00, 0 => 0x80, +100 => 0xFF</summary>
	public byte ModY = 0x80;

	internal ushort ReadChannelIndex;

	public FLPatternNote(FLWriteChannel chan)
	{
		ReadChannel = null!;
		WriteChannel = chan;
	}
	internal FLPatternNote(EndianBinaryReader r)
	{
		AbsoluteTick = r.ReadUInt32();

		Slide = r.ReadByte() == 8; // Not sure on the other values...
		r.ReadByte(); // 0x40
		ReadChannelIndex = r.ReadUInt16();
		ReadChannel = null!;
		WriteChannel = null!;

		DurationTicks = r.ReadUInt32();
		Key = r.ReadByte();
		r.ReadByte(); // 0
		r.ReadByte(); // 0
		r.ReadByte(); // 0

		Pitch = r.ReadByte();
		r.ReadByte(); // 0
		Release = r.ReadByte();
		byte b = r.ReadByte();
		Portamento = (b & 0x10) != 0;
		Color = (byte)(b & 0xF);

		Panpot = r.ReadByte();
		Velocity = r.ReadByte();
		ModX = r.ReadByte();
		ModY = r.ReadByte();
	}

	internal readonly void Write(EndianBinaryWriter w)
	{
		w.WriteUInt32(AbsoluteTick);

		w.WriteByte((byte)(Slide ? 8 : 0));
		w.WriteByte(0x40);
		w.WriteUInt16(WriteChannel.Index);

		w.WriteUInt32(DurationTicks);
		w.WriteUInt32(Key);

		w.WriteByte(Pitch);
		w.WriteByte(0);
		w.WriteByte(Release);
		// 0 through F are colors with no porta, 0x10 is color0 with porta, 0x1F is colorF with porta
		w.WriteByte((byte)(Color + (Portamento ? 0x10 : 0)));

		w.WriteByte(Panpot);
		w.WriteByte(Velocity);
		w.WriteByte(ModX);
		w.WriteByte(ModY);
	}
}
