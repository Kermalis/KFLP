namespace Kermalis.FLP;

public sealed class FLReadChannel
{
	internal readonly ushort Index;

	public string Name;

	internal FLReadChannel(ushort index)
	{
		Index = index;

		Name = string.Empty;
	}

	public override string ToString()
	{
		return Name;
	}
}
