using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BlockadeState
	{
		private readonly BoardCalculator _boardCalculator;

		private readonly Cell[][] _board;
		private readonly Location[] _playerLocations;
		private readonly List<int> _finalResults;

		private int _currentPlayer;
		private int _turn;

		public int Rows { get { return this._board.Length; } }
		public int Cols { get { return this._board[0].Length; } }

		public int CurrentPlayer { get { return this._currentPlayer; } }

		private BlockadeState(
			BoardCalculator boardCalculator,
			Cell[][] board,
			Location[] playerLocations,
			List<int> finalResults,
			int currentPlayer,
			int turn)
		{
			this._boardCalculator = boardCalculator;
			this._board = board;
			this._playerLocations = playerLocations;
			this._finalResults = finalResults;
			this._currentPlayer = currentPlayer;
			this._turn = turn;
		}

		private static BlockadeState CreateFromConfiguration(
			BlockadeConfiguration configuration,
			BoardCalculator boardCalculator)
		{
			var board = Enumerable.Range(0, configuration.Rows)
				.Select(row => Enumerable.Range(0, configuration.Cols)
					.Select(col => Cell.EmptyCell)
					.ToArray())
				.ToArray();
			var playerLocations = configuration.StartingLocations
				.Select(t => Location.Create(t.Item1, t.Item2))
				.ToArray();
			var finalResults = new List<int>(capacity: configuration.Players.Count);
			var currentPlayer = 0;
			var turn = 1;

			// place players at their initial positions
			foreach (var i in Enumerable.Range(0, playerLocations.Length))
			{
				var location = playerLocations[i];
				board[location.Row][location.Col] = Cell.MakeOccupiedCell(player: i, turn: 0);
			}

			return new BlockadeState(boardCalculator, board, playerLocations, finalResults, currentPlayer, turn);
		}

		public BlockadeState Clone()
		{
			// these rely on the underlying values (cell, location) to be immutable
			var newBoard = this.GetBoard();
			var newPlayerLocations = this._playerLocations.ToArray();
			var newFinalResults = this._finalResults.ToList();

			return new BlockadeState(this._boardCalculator, newBoard, newPlayerLocations, newFinalResults, this._currentPlayer, this._turn);
		}

		public IEnumerable<Move> GetMoves()
		{
			Throw.InvalidIf(this.IsGameOver(), "game is over");

			return this._boardCalculator.GetD1Neighbors(this.AsReadOnly(), this._playerLocations[this._currentPlayer], distance: 1)
				.Where(l => !this._board[l.Row][l.Col].Player.HasValue)
				.Select(Move.Create);
		}

		public void MakeMove(Move move)
		{
			Throw.InvalidIf(this.IsGameOver(), "game is over");

			this._board[move.Location.Row][move.Location.Col] = Cell.MakeOccupiedCell(this._currentPlayer, this._turn);
			this._playerLocations[this._currentPlayer] = move.Location;

			var originalPlayer = this._currentPlayer;
			while (!this.IsGameOver())
			{
				this._currentPlayer++;
				if (this._currentPlayer == this._playerLocations.Length)
				{
					this._currentPlayer = 0;
					this._turn++;
				}

				// we've made it back to the beginning, so the game is over
				if (this._currentPlayer == originalPlayer)
				{
					this._finalResults.Add(this._currentPlayer);
					Throw.InvalidIf(this._finalResults.Count != this._playerLocations.Length, "expected game to be over");
					return;
				}

				// we found a still alive player who has a move
				if (this._playerLocations[this._currentPlayer] != null && this.GetMoves().Any())
				{
					return;
				}

				// this is a player who was alive but now has no moves
				if (this._playerLocations[this._currentPlayer] != null)
				{
					this._playerLocations[this._currentPlayer] = null;
					this._finalResults.Add(this._currentPlayer);
				}

				// this is an already or freshly dead player, so loop
			}
		}

		public bool IsGameOver()
		{
			return this._finalResults.Count == this._playerLocations.Length;
		}

		public BlockadeResult ToResult()
		{
			Throw.InvalidIf(!this.IsGameOver(), "game is not over");

			return new BlockadeResult(
				finalOrdering: this._finalResults.AsEnumerable().Reverse(),
				board: this.GetBoard());
		}

		public ReadOnlyBlockadeState AsReadOnly()
		{
			return new ReadOnlyBlockadeState(this);
		}

		public Cell[][] GetBoard()
		{
			return this._board.Select(r => r.ToArray()).ToArray();
		}

		public Cell GetCell(Location location)
		{
			return this._board[location.Row][location.Col];
		}

		public Location GetCurrentLocationOfPlayer(int player)
		{
			return this._playerLocations[player];
		}

		public class BlockadeStateFactory
		{
			private readonly BoardCalculator _boardCalculator;

			public BlockadeStateFactory(BoardCalculator boardCalculator)
			{
				this._boardCalculator = boardCalculator;
			}

			public BlockadeState CreateFromConfiguration(BlockadeConfiguration configuration)
			{
				return BlockadeState.CreateFromConfiguration(configuration, this._boardCalculator);
			}
		}
	}

	public class Move : IEquatable<Move>
	{
		public Location Location { get; private set; }

		private Move(Location location)
		{
			this.Location = location;
		}

		public static Move Create(Location location)
		{
			return new Move(location);
		}

		public bool Equals(Move other)
		{
			return this.Location.Equals(other.Location);
		}

		public override int GetHashCode()
		{
			return this.Location.GetHashCode();
		}
	}

	public class Location : IEquatable<Location>
	{
		public int Row { get; private set; }
		public int Col { get; private set; }

		private Location(int row, int col)
		{
			this.Row = row;
			this.Col = col;
		}

		public static Location Create(int row, int col)
		{
			return new Location(row, col);
		}

		public bool Equals(Location other)
		{
			return Tuple.Create(this.Row, this.Col)
				.Equals(Tuple.Create(other.Row, other.Col));
		}

		public override int GetHashCode()
		{
			return Tuple.Create(this.Row, this.Col).GetHashCode();
		}
	}

	public class Cell : IEquatable<Cell>
	{
		public int? Player { get; private set; }
		public int? Turn { get; private set; }

		private Cell(int? player, int? turn)
		{
			this.Player = player;
			this.Turn = turn;
		}

		public static Cell MakeOccupiedCell(int player, int turn)
		{
			return new Cell(player, turn);
		}

		public static Cell EmptyCell = new Cell(player: default(int?), turn: default(int?));

		public bool Equals(Cell other)
		{
			return Tuple.Create(this.Player, this.Turn)
				.Equals(Tuple.Create(other.Player, other.Turn));
		}

		public override int GetHashCode()
		{
			return Tuple.Create(this.Player, this.Turn).GetHashCode();
		}
	}
}

