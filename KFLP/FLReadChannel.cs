namespace Kermalis.FLP;

public sealed class FLReadChannel
{
	internal readonly ushort Index;

	public string Name;
	public FLAutomationData? AutoData;

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
