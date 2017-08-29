using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace blockade.Controllers
{
	public class BlockadeController : Controller
	{
		private readonly PlayerProvider _playerProvider;

		public BlockadeController(PlayerProvider playerProvider)
		{
			this._playerProvider = playerProvider;
		}

		public ActionResult Index()
		{
			return this.View("blockade", new BlockadeModel 
			{ 
				Players = this._playerProvider.GetPlayerDescriptions()
			});
		}

		[HttpPost]
		public JsonResult PlayOneGame()
		{
			// have to use this.Request["data"] instead of parameters to extract data from the post because mono
			var data = JsonConvert.DeserializeObject<PlayOneGameRequest>(this.Request["data"]);

			var configuration = BlockadeConfiguration.MakeConfiguration(
				rows: data.Configuration.Rows,
				cols: data.Configuration.Cols,
				playersWithStartingLocation: data.Configuration.PlayersWithStartingPosition
					.Select(a => Tuple.Create(this._playerProvider.GetPlayer(a.Name), a.StartingLocation))
					.ToList());

			var game = new BlockadeGame(configuration);
			var result = game.Run();

			var board = result.Board.To2dArray((player, turn) => Tuple.Create(player, turn));
			return this.Json(new PlayOneGameResponse
			{
				Board = board,
				ResultsWithFinalTurn = result.PlayerOrdering
					.Select(playerI => Tuple.Create(playerI, board.SelectMany(row => row)
						.Where(t => t != null && t.Item1 == playerI)
						.Max(t => t.Item2)))
					.ToArray()
			});
		}
	
		[HttpPost]
		public JsonResult PlayManyGames()
		{
			// have to use this.Request["data"] instead of parameters to extract data from the post because mono
			var data = JsonConvert.DeserializeObject<PlayManyGamesRequest>(this.Request["data"]);

			var configuration = BlockadeConfiguration.MakeConfiguration(
				rows: data.Configuration.Rows,
				cols: data.Configuration.Cols,
				playersWithStartingLocation: data.Configuration.PlayersWithStartingPosition
					.Select(a => Tuple.Create(this._playerProvider.GetPlayer(a.Name), a.StartingLocation))
					.ToList());

			// todo parallel?
			var results = Enumerable.Range(0, data.NumGames)
				.Select(_ => new BlockadeGame(configuration).Run())
				.ToList();

			return this.Json(new PlayManyGamesResult
			{
				WinPercentages = Enumerable.Range(0, configuration.Players.Count)
					.Select(playerI => Enumerable.Range(0, configuration.Players.Count)
						.Select(resultI => results.Count(r => r.PlayerOrdering[resultI] == playerI) * 1.0 / data.NumGames)
						.ToArray())
					.ToArray()
			});
		}

		private class GameConfigurationRequest
		{
			public int Rows { get; set; }
			public int Cols { get; set; }
			public PlayerWithStartingPosition[] PlayersWithStartingPosition { get; set; }

			public class PlayerWithStartingPosition
			{
				public string Name { get; set; }
				public Tuple<int, int> StartingLocation { get; set; }
			}
		}

		private class PlayOneGameRequest
		{
			public GameConfigurationRequest Configuration { get; set; }
		}

		private class PlayOneGameResponse
		{
			public Tuple<int, int>[][] Board { get; set; }
			public Tuple<int, int>[] ResultsWithFinalTurn { get; set; }
		}

		private class PlayManyGamesRequest
		{
			public GameConfigurationRequest Configuration { get; set; }
			public int NumGames { get; set; }
		}

		private class PlayManyGamesResult
		{
			// [player][position]
			public double[][] WinPercentages { get; set; }
		}
	}
}
