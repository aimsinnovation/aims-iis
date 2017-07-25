using System;
using System.IO;

namespace Aims.IISAgent.Module
{
	public class Message
	{
		public string Site { get; set; }

		public byte Code { get; set; }

		public DateTime DateTime { get; set; }

		public string StatType { get; set; }

		public Message()
		{
			DateTime = new DateTime();
		}

		public static Message Deserialize(byte[] buffer, int offset, int count)
		{
			using (var stream = new MemoryStream(buffer, offset, count))
			using (var reader = new BinaryReader(stream))
			{
				return new Message
				{
					DateTime = new DateTime(reader.ReadInt64()),
					StatType = reader.ReadString(),
				};
			}
		}

		public byte[] Serialize()
		{
			using (var stream = new MemoryStream())
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(DateTime.Ticks);
				writer.Write(StatType);
				return stream.ToArray();
			}
		}
	}
}