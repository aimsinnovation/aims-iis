﻿using System;
using System.IO;

namespace Aims.IISAgent.Pipes.Tools.Serialization
{
    internal class UshortSerializer : PrimitiveSerializer<ushort>
    {
        internal override void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)DataType.Ushort);
        }

        internal override void SerializeDataSpecific(object obj, BinaryWriter writer)
        {
            writer.Write((ushort)obj);
        }

        internal override object DeserializeDataSpecific(Type type, BinaryReader reader)
        {
            return reader.ReadUInt16();
        }
    }
}