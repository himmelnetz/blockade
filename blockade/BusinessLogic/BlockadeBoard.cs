using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BlockadeBoard
	{
		public int Rows { get; private set; }
		public int Cols { get; private set; }

		private Tuple<int, int>[][] _board;

		public BlockadeBoard (int rows, int cols)
		{
			this.Rows = rows;
			this.Cols = cols;
			this.InitializeBlankBoard();
		}

		public void PlacePlayer(Tuple<int, int> location, int player, int turn) 
		{
			if (this._board[location.Item1][location.Item2] != null)
			{
				throw new InvalidOperationException("trying to place player where a player already exists");
			}
			this._board[location.Item1][location.Item2] = Tuple.Create(player, turn);
		}

		public List<Tuple<int, int>> GetMovesFromLocation(Tuple<int, int> location)
		{
			return this.GetD1Neighbors(location, distance: 1)
				.Where(l => this._board[l.Item1][l.Item2] == null)
					.ToList();
		}

		public IEnumerable<Tuple<int, int>> GetD1Neighbors(Tuple<int, int> location, int distance)
		{
			return Enumerable.Range(0, distance + 1)
				.Select(x => Tuple.Create(x, distance - x))
					.SelectMany(t => new[] { t, Tuple.Create(-t.Item1, t.Item2), Tuple.Create(t.Item1, -t.Item2), Tuple.Create(-t.Item1, -t.Item2) })
					.Select(delta => Tuple.Create(location.Item1 + delta.Item1, location.Item2 + delta.Item2))
					.Where(this.IsLocationOnBoard)
					.Distinct();
		}

		public T[][] To2dArray<T>(Func<int, int, T> playerAndTurnToT)
		{
			return this._board.Select(r => r.Select(t => t == null ? default(T) : playerAndTurnToT(t.Item1, t.Item2)).ToArray()).ToArray();
		}

		public int? GetPlayerAtLocation(Tuple<int, int> location)
		{
			return !this.IsLocationOnBoard(location) || this._board[location.Item1][location.Item2] == null
				? default(int?)
					: this._board[location.Item1][location.Item2].Item1;
		}

		public BlockadeBoard Clone()
		{
			var other = new BlockadeBoard(this.Rows, this.Cols);
			foreach (var row in Enumerable.Range(0, this.Rows))
			{
				foreach (var col in Enumerable.Range(0, this.Cols))
				{
					other._board[row][col] = this._board[row][col];
				}
			}
			return other;
		}

		private bool IsLocationOnBoard(Tuple<int, int> location)
		{
			return location.Item1 >= 0 && location.Item1 < this.Rows
				&& location.Item2 >= 0 && location.Item2 < this.Cols;
		}

		private void InitializeBlankBoard()
		{
			this._board = Enumerable.Range(0, this.Rows)
				.Select(_ => Enumerable.Range(0, this.Cols)
				        .Select(__ => default(Tuple<int, int>))
				        .ToArray())
					.ToArray();
		}
	}
}

