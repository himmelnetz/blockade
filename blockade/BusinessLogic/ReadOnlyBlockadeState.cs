using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class ReadOnlyBlockadeState
	{
		private readonly BlockadeState _state;

		public ReadOnlyBlockadeState(BlockadeState state)
		{
			this._state = state;
		}

		public int Rows { get { return this._state.Rows; } }
		public int Cols { get { return this._state.Cols; } }
		public int CurrentPlayer { get { return this._state.CurrentPlayer; } }

		public IEnumerable<Move> GetMoves()
		{
			return this._state.GetMoves();
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
	}
}

