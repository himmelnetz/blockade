using System;
using System.Collections.Generic;

namespace blockade
{
	public class SophiePlayer : IBlockadePlayer
	{
		public SophiePlayer()
		{
		}

		public static BlockadePlayerDescription GetPlayerDescription()
		{
			return new BlockadePlayerDescription
			{
				Name = "Sophie",
				Description = "STILL IN DEVELOPMENT"
			};
		}

		public int PickMove(List<Move> moves, ReadOnlyBlockadeState state)
		{
			return 0;
		}
	}
}

