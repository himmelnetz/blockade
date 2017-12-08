using System;

namespace blockade
{
	public static class Throw
	{
		public static void If(bool condition, string message = "")
		{
			if (condition) 
			{
				throw new ArgumentException(message);
			}
		}

		public static void InvalidIf(bool condition, string message = "")
		{
			if (condition) 
			{
				throw new InvalidOperationException(message);
			}
		}

		public static void IfNull(object value, string message = "")
		{
			if (value == null)
			{
				throw new ArgumentNullException(message);
			}
		}

		public static void IfOutOfRange(int value, int? lo, int? hi, string message = "")
		{
			if ((lo.HasValue && value < lo.Value) || (hi.HasValue && value >= hi.Value))
			{
				throw new ArgumentOutOfRangeException(message);
			}
		}
	}
}

