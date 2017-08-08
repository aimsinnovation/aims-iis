using System;
using System.IO;

namespace Aims.IISAgent.Module.Pipes
{
	public class Message
	{
		public Message()
		{
			DateTime = new DateTime();
		}

		public int Code { get; set; }

		public DateTime DateTime { get; set; }

		public string Domain { get; set; }

		public int Port { get; set; }

		public string Scheme { get; set; }

		//segment enumerated with 1
		public string Segment2 { get; set; }

		public string StatType { get; set; }

		public static Message Deserialize(byte[] buffer, int offset, int count)
		{
			using (var stream = new MemoryStream(buffer, offset, count))
			using (var reader = new BinaryReader(stream))
			{
				return new Message
				{
					Segment2 = reader.ReadString(),
					Code = reader.ReadInt32(),
					DateTime = new DateTime(reader.ReadInt64()),
					Domain = reader.ReadString(),
					Port = reader.ReadInt32(),
					Scheme = reader.ReadString(),
					StatType = reader.ReadString(),
				};
			}
		}

		public byte[] Serialize()
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(Segment2);
				writer.Write(Code);
				writer.Write(DateTime.Ticks);
				writer.Write(Domain);
				writer.Write(Port);
				writer.Write(Scheme);
				writer.Write(StatType);
				return stream.ToArray();
			}
		}
	}
}