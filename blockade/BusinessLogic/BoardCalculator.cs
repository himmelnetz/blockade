using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BoardCalculator
	{
		private readonly ReadOnlyBlockadeState _state;

		public delegate BoardCalculator Factory(ReadOnlyBlockadeState state);

		public BoardCalculator(ReadOnlyBlockadeState state)
		{
			this._state = state;
		}

		public IEnumerable<Location> GetD1Neighbors(Location location, int distance)
		{
			if (distance == 1)
			{
				return new[]
				{
					Location.Create(location.Row - 1, location.Col),
					Location.Create(location.Row + 1, location.Col),
					Location.Create(location.Row, location.Col - 1),
					Location.Create(location.Row, location.Col + 1)
				}.Where(l => this.IsLocationOnBoard(l));
			}

			return Enumerable.Range(0, distance + 1)
				.Select(x => Tuple.Create(x, distance - x))
				.SelectMany(t => new[] { t, Tuple.Create(-t.Item1, t.Item2), Tuple.Create(t.Item1, -t.Item2), Tuple.Create(-t.Item1, -t.Item2) })
				.Select(delta => Location.Create(row: location.Row + delta.Item1, col: location.Col + delta.Item2))
				.Where(l => this.IsLocationOnBoard(l))
				.Distinct();
		}

		private bool IsLocationOnBoard(Location location)
		{
			return location.Row >= 0 && location.Row < this._state.Rows
				&& location.Col >= 0 && location.Col < this._state.Cols;
		}
	}
}

