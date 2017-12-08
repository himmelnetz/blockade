using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace blockade
{
	public class BlockadeResult
	{
		public IReadOnlyList<int> FinalOrdering { get; private set; }
		public Grid<Cell> Board { get; private set; }

		public BlockadeResult (IEnumerable<int> finalOrdering, Grid<Cell> board)
		{
			this.FinalOrdering = finalOrdering.ToList();
			this.Board = board.Clone();
		}
	}
}

