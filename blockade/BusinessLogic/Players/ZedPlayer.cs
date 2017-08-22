using System;
using System.Collections.Generic;

namespace blockade
{
	public class ZedPlayer : BlockadePlayer
	{
		private readonly Random _random;

		public ZedPlayer(Random random)
		{
			this._random = random;
		}

		public int PickMove(List<Tuple<int, int>> locations, ReadOnlyBlockadeBoard board, int turn)
		{
			return this._random.Next(locations.Count);
		}
	}
}
