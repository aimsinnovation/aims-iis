using System.Text;

namespace Aims.IISAgent
{
	public static class ArrayExtensions
	{
		private static readonly string[] LookupTable;

		static ArrayExtensions()
		{
			LookupTable = new string[256];
			for (int i = 0; i < 256; i++)
			{
				LookupTable[i] = i.ToString("X2");
			}
		}

		public static string ToHex(this byte[] array)
		{
			var stringBuilder = new StringBuilder();
			foreach (var b in array)
			{
				stringBuilder.Append(LookupTable[b]);
			}

			return stringBuilder.ToString();
		}
	}
}