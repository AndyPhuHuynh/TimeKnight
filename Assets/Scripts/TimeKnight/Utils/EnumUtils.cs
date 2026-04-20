using System;

namespace TimeKnight.Utils
{
	public static class EnumUtils
	{
		public static T[] GetEnumValues<T>() where T : struct, Enum 
		{
			return (Enum.GetValues(typeof(T)) as T[])!;
		}
	}
}