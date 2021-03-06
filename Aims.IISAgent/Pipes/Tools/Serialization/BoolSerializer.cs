﻿using System;
using System.IO;

namespace Aims.IISAgent.Pipes.Tools.Serialization
{
	internal class BoolSerializer : PrimitiveSerializer<bool>
	{
		internal override void Serialize(BinaryWriter writer)
		{
			writer.Write((byte)DataType.Bool);
		}

		internal override void SerializeDataSpecific(object obj, BinaryWriter writer)
		{
			writer.Write((bool)obj);
		}

		internal override object DeserializeDataSpecific(Type type, BinaryReader reader)
		{
			return reader.ReadBoolean();
		}
	}
}