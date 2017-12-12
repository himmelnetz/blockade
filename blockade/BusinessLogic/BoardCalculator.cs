using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BoardCalculator
	{
		private readonly ReadOnlyBlockadeState _state;

		public delegate BoardCalculator Factory(ReadOnlyBlockadeState state);

		private readonly Lazy<Grid<int?>> _connectedComponentLabels;

		public BoardCalculator(ReadOnlyBlockadeState state)
		{
			this._state = state;

			this._connectedComponentLabels = new Lazy<Grid<int?>>(() => ComputeConnectedComponents(state));
		}

		public IEnumerable<Location> GetD1Neighbors(Location location, int distance)
		{
			return GetD1Neighbors(this._state, location, distance);
		}

		public int GetReachableCellCount(Location location)
		{
			var labels = this._connectedComponentLabels.Value;
			var thisLabel = labels[location];

			// i dont expect to ever call this with a filled location... if that ends up needing to
			// happen, should consider what to return (0, default(int?))
			Throw.If(!thisLabel.HasValue);

			return labels.Enumerate((_, label) => label)
				.Count(label => label == thisLabel);
		}

		// static so it can be shared with other statics
		private static IEnumerable<Location> GetD1Neighbors(ReadOnlyBlockadeState state, Location location, int distance)
		{
			if (distance == 1)
			{
				return new[]
				{
					Location.Create(location.Row - 1, location.Col),
					Location.Create(location.Row + 1, location.Col),
					Location.Create(location.Row, location.Col - 1),
					Location.Create(location.Row, location.Col + 1)
				}.Where(l => IsLocationOnBoard(state, l));
			}

			return Enumerable.Range(0, distance + 1)
				.Select(x => Tuple.Create(x, distance - x))
					.SelectMany(t => new[] { t, Tuple.Create(-t.Item1, t.Item2), Tuple.Create(t.Item1, -t.Item2), Tuple.Create(-t.Item1, -t.Item2) })
					.Select(delta => Location.Create(row: location.Row + delta.Item1, col: location.Col + delta.Item2))
					.Where(l => IsLocationOnBoard(state, l))
					.Distinct();
		}

		// static so it can be used in a lazy
		private static Grid<int?> ComputeConnectedComponents(ReadOnlyBlockadeState state)
		{
			var labels = Grid.Create(state.Rows, state.Cols, (row, col) => default(int?));
			var currentLabel = 0;
			state.GetBoard().ForEach((location, cell) =>
			{
				if (cell.IsEmpty() && !labels[location].HasValue) 
				{
					ApplyLabel(state, labels, location, currentLabel);
					currentLabel++;
				}
			});
			return labels;
		}

		private static void ApplyLabel(ReadOnlyBlockadeState state, Grid<int?> labels, Location location, int label)
		{
			Throw.If(labels[location].HasValue);

			labels[location] = label;
			GetD1Neighbors(state, location, distance: 1)
				.ToList()
				.ForEach(newLocation =>
				{
					// not using Where() because we need to evaluate condition immediately before recursing
					if (!labels[newLocation].HasValue && state.GetCell(newLocation).IsEmpty())
					{
						ApplyLabel(state, labels, newLocation, label);
					}
				});
		}

		private static bool IsLocationOnBoard(ReadOnlyBlockadeState state, Location location)
		{
			return location.Row >= 0 && location.Row < state.Rows
				&& location.Col >= 0 && location.Col < state.Cols;
		}
	}
}

