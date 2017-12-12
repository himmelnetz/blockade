using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class ReadOnlyBlockadeState
	{
		private readonly BlockadeState _state;

		private Lazy<Grid<Cell>> _board;
		private Lazy<BoardCalculator> _boardCalculator;

		public ReadOnlyBlockadeState(BlockadeState state)
		{
			this._state = state;

			this._board = new Lazy<Grid<Cell>>(() => state.GetBoard());
			this._boardCalculator = new Lazy<BoardCalculator>(() => state.GetBoardCalculator());

		}

		public int Rows { get { return this._state.Rows; } }
		public int Cols { get { return this._state.Cols; } }
		public int CurrentPlayer { get { return this._state.CurrentPlayer; } }

		public IEnumerable<Move> GetMoves()
		{
			return this._state.GetMoves();
		}

		public IEnumerable<Move> GetMoves(int player)
		{
			return this._state.GetMoves(player);
		}

		public ReadOnlyBlockadeState MakeMove(Move move)
		{
			var other = this._state.Clone();
			other.MakeMove(move);
			return new ReadOnlyBlockadeState(other);
		}

		public Cell GetCell(Location location)
		{
			return this._state.GetCell(location);
		}

		public Location GetCurrentLocationOfPlayer(int player)
		{
			return this._state.GetCurrentLocationOfPlayer(player);
		}

		public bool IsGameOver()
		{
			return this._state.IsGameOver();
		}

		public Grid<Cell> GetBoard()
		{
			return this._board.Value;
		}

		public BoardCalculator GetBoardCalculator()
		{
			return this._boardCalculator.Value;
		}
	}
}

