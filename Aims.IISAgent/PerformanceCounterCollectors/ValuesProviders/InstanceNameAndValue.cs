using System.IO;

namespace Aims.IISAgent.PerformanceCounterCollectors.ValuesProviders
{
	public struct InstanceNameAndValue
	{
		public string InstanceName;
		public double Value;
	}

	public static class ReadWriteExtention
	{
		public static void Write(this BinaryWriter writer, InstanceNameAndValue value)
		{
			writer.Write(value.InstanceName);
			writer.Write(value.Value);
		}

		public static InstanceNameAndValue ReadInstanceNameAndValue(this BinaryReader reader)
		{
			var answer = new InstanceNameAndValue
			{
				InstanceName = reader.ReadString(),
				Value = reader.ReadDouble()
			};
			return answer;
		}
	}
}