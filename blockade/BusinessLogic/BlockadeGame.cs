using System;
using System.Collections.Generic;
using System.Linq;

namespace blockade
{
	public class BlockadeGame
	{
		private readonly BlockadeConfiguration _configuration;
		
		private readonly BlockadeState.BlockadeStateFactory _blockadeStateFactory;

		public delegate BlockadeGame Factory(BlockadeConfiguration configuration);

		public BlockadeGame(
			BlockadeConfiguration configuration,
			BlockadeState.BlockadeStateFactory blockadeStateFactory)
		{
			this._configuration = configuration;
			this._blockadeStateFactory = blockadeStateFactory;
		}

		public BlockadeResult Run()
		{
			var state = this._blockadeStateFactory.CreateFromConfiguration(this._configuration);

			while (!state.IsGameOver())
			{
				var moves = state.GetMoves().ToList();
				var moveI = moves.Count > 1
					? this._configuration.Players[state.CurrentPlayer]
						.PickMove(moves, state.AsReadOnly())
					: 0;
				state.MakeMove(moves[moveI]);
			}

			return state.ToResult();
		}
	}
}

