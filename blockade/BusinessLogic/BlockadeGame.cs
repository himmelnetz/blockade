using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BlockadeGame
	{
		private const int BoardRows = 10;
		private const int BoardCols = 16;

		private readonly List<BlockadePlayer> _players;
		private readonly BlockadeBoard _board;

		private Tuple<int, int>[] _playerLocations;
		private List<int> _finalOrdering;

		public BlockadeGame (List<BlockadePlayer> players)
		{
			this._players = players;
			this._board = new BlockadeBoard(BoardRows, BoardCols);
		}

		public BlockadeResult Run()
		{
			this.InitializeGame();
			var turn = 1;

			while (this._playerLocations.Where(t => t != null).Count() > 1)
			{
				this.StepGame(turn);
				turn++;
			}

			var winningIndex = Enumerable.Range(0, this._players.Count)
				.Where(i => this._playerLocations[i] != null)
					.Select(i => (int?) i)
					.SingleOrDefault();
			if (winningIndex.HasValue)
			{
				this._finalOrdering.Add(winningIndex.Value);
			}
			this._finalOrdering.Reverse();

			return new BlockadeResult(
				this._finalOrdering.ToList(),
				new ReadOnlyBlockadeBoard(this._board));
		}

		private void StepGame(int turn)
		{
			foreach (var i in Enumerable.Range(0, this._players.Count).Where(i => this._playerLocations[i] != null))
			{
				var possibleMoves = this._board.GetMovesFromLocation(this._playerLocations[i]);
				if (possibleMoves.Count == 0)
				{
					this._playerLocations[i] = null;
					this._finalOrdering.Add(i);
				}
				else
				{
					var board = new ReadOnlyBlockadeBoard(this._board);
					var move = possibleMoves.Count > 1
						? possibleMoves[this._players[i].PickMove(possibleMoves, board, turn)]
						: possibleMoves[0];
					this._board.PlacePlayer(move, i, turn);
					this._playerLocations[i] = move;
				}
			}
		}

		private void InitializeGame()
		{
			this._finalOrdering = new List<int>();
			// SUPER HARD CODED!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
			this._board.PlacePlayer(Tuple.Create(0, 0), 0, 0);
			this._board.PlacePlayer(Tuple.Create(0, 15), 1, 0);
			//this._board.PlacePlayer(Tuple.Create(9, 0), 2, 0);
			//this._board.PlacePlayer(Tuple.Create(9, 15), 3, 0);
			//this._board.PlacePlayer(Tuple.Create(0, 8), 4, 0);
			//this._board.PlacePlayer(Tuple.Create(9, 8), 5, 0);
			this._playerLocations = new[]
			{ 
				Tuple.Create(0, 0),
				Tuple.Create(0, 15)//,
				//Tuple.Create(9, 0),
				//Tuple.Create(9, 15),
				//Tuple.Create(0, 8),
				//Tuple.Create(9, 8)
			};
		}
	}
}

