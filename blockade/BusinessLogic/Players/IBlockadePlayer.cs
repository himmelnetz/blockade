using System;
using System.Collections.Generic;

namespace blockade
{
	public interface IBlockadePlayer
	{
		int PickMove(List<Move> moves, ReadOnlyBlockadeState state);
	}

	public class BlockadePlayerDescription
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}
}

