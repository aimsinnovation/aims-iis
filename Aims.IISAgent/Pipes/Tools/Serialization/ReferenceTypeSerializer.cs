using System;

namespace Aims.IISAgent.Pipes.Tools.Serialization
{
    internal abstract class ReferenceTypeSerializer : Serializer
    {
        internal override bool IsDataNullable
        {
            get { return true; }
            set { throw new InvalidOperationException(); }
        }
    }
}