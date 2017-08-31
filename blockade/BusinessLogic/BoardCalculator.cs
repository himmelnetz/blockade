using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BoardCalculator
	{
		public BoardCalculator()
		{
		}

		public IEnumerable<Location> GetD1Neighbors(ReadOnlyBlockadeState state, Location location, int distance)
		{
			return Enumerable.Range(0, distance + 1)
				.Select(x => Tuple.Create(x, distance - x))
				.SelectMany(t => new[] { t, Tuple.Create(-t.Item1, t.Item2), Tuple.Create(t.Item1, -t.Item2), Tuple.Create(-t.Item1, -t.Item2) })
				.Select(delta => Location.Create(row: location.Row + delta.Item1, col: location.Col + delta.Item2))
				.Where(l => this.IsLocationOnBoard(state, l))
				.Distinct();
		}

		private bool IsLocationOnBoard(ReadOnlyBlockadeState state, Location location)
		{
			return location.Row >= 0 && location.Row < state.Rows
				&& location.Col >= 0 && location.Col < state.Cols;
		}
	}
}

