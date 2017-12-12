using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class Grid<T> : IEqualityComparer<Grid<T>>
		where T : IEqualityComparer<T>
	{
		private readonly List<List<T>> _grid;

		public int Rows { get { return this._grid.Count; } }
		public int Cols { get { return this._grid[0].Count; } }

		internal Grid(List<List<T>> grid)
		{
			this._grid = grid;
		}

		public T this[Location location]
		{
			get { return this._grid[location.Row][location.Col]; }
			set { this._grid[location.Row][location.Col] = value; }
		}

		public Grid<T> Clone()
		{
			return new Grid<T>(this._grid.Select(r => r.ToList()).ToList());
		}

		public T[,] To2dArray()
		{
			var result = new T[this.Rows, this.Cols];
			this.ForEach((location, t) => result[location.Row, location.Col] = t);
			return result;
		}

		public void ForEach(Action<Location, T> action)
		{
			foreach (var locationWithValue in this.Enumerate((location, t) => Tuple.Create(location, t)))
			{
				action(locationWithValue.Item1, locationWithValue.Item2);
			}
		}

		public IEnumerable<TOut> Enumerate<TOut>(Func<Location, T, TOut> selector)
		{
			return this._grid.SelectMany((row, rowI) => row.Select((t, colI) => selector(Location.Create(rowI, colI), t)));
		}

		public override string ToString()
		{
			return string.Join("\n", this._grid.Select(row => string.Join(", ", row.Select(t => t.ToString()))));
		}

		public bool Equals(Grid<T> @this, Grid<T> other)
		{
			throw new NotImplementedException();
		}

		public int GetHashCode(Grid<T> @this)
		{
			return @this.Enumerate((location, t) => Tuple.Create(location, t))
				// bolch's effective jave list hash
				.Aggregate(seed: 17, func: (acc, tuple) => acc * 31 + tuple.GetHashCode());
		}
	}

	// necessary 2nd class for type inference on static factory
	// https://stackoverflow.com/questions/31195524/c-sharp-generics-infer-type-of-static-factory-members
	public static class Grid
	{
		public static Grid<T> Create<T>(int rows, int cols, Func<int, int, T> selector)
			where T : IEqualityComparer<T>
		{
			Throw.IfOutOfRange(rows, lo: 0, hi: default(int?));
			Throw.IfOutOfRange(cols, lo: 0, hi: default(int?));

			return new Grid<T>(Enumerable.Range(0, rows)
				.Select(row => Enumerable.Range(0, cols)
					.Select(col => selector(row, col))
					.ToList())
				.ToList());
		}
	}
}

