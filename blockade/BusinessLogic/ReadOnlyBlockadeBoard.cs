using System;
using System.Collections.Generic;

namespace blockade
{
	public class ReadOnlyBlockadeBoard
	{
		public int Rows { get { return this._board.Rows; } }
		public int Cols { get { return this._board.Cols; } }

		private readonly BlockadeBoard _board;

		public ReadOnlyBlockadeBoard (BlockadeBoard board)
		{
			this._board = board;
		}

		public List<Tuple<int, int>> GetMovesFromLocation(Tuple<int, int> location)
		{
			return this._board.GetMovesFromLocation(location);
		}

		public T[][] To2dArray<T>(Func<int, int, T> playerAndTurnToT)
		{
			return this._board.To2dArray(playerAndTurnToT);
		}

		public IEnumerable<Tuple<int, int>> GetD1Neighbors(Tuple<int, int> location, int distance)
		{
			return this._board.GetD1Neighbors(location, distance);
		}

		public ReadOnlyBlockadeBoard CloneAndPlacePlayer(Tuple<int, int> location, int player, int turn)
		{
			var other = this._board.Clone();
			other.PlacePlayer(location, player, turn);
			return new ReadOnlyBlockadeBoard(other);
		}

		public int? GetPlayerAtLocation(Tuple<int, int> location)
		{
			return this._board.GetPlayerAtLocation(location);
		}
	}
}

