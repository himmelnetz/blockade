using System;
using System.Collections.Generic;

namespace blockade
{
	public interface BlockadePlayer
	{
		int PickMove(List<Tuple<int, int>> locations, ReadOnlyBlockadeBoard board, int turn);
	}
}

