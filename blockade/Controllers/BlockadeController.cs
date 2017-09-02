using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;

namespace blockade.Controllers
{
	public class BlockadeController : Controller
	{
		private readonly PlayerProvider _playerProvider;
		private readonly BlockadeGame.Factory _blockadeGameFactory;

		public BlockadeController(
			PlayerProvider playerProvider,
			BlockadeGame.Factory blockadeGameFactory)
		{
			this._playerProvider = playerProvider;
			this._blockadeGameFactory = blockadeGameFactory;
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

			var game = this._blockadeGameFactory(configuration);
			var result = game.Run();

			return this.Json(new PlayOneGameResponse
			{
				Board = result.Board,
				ResultsWithFinalTurn = result.FinalOrdering
					.Select(playerI => Tuple.Create(playerI, result.Board.SelectMany(row => row)
						.Where(c => c.Player == playerI)
						.Select(c => c.Turn.Value)
						.Max()))
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

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			// todo parallel?
			var results = Enumerable.Range(0, data.NumGames)
				.Select(_ => this._blockadeGameFactory(configuration).Run())
				.ToList();

			stopwatch.Stop();

			return this.Json(new PlayManyGamesResult
			{
				WinPercentages = Enumerable.Range(0, configuration.Players.Count)
					.Select(playerI => Enumerable.Range(0, configuration.Players.Count)
						.Select(resultI => results.Count(r => r.FinalOrdering[resultI] == playerI) * 1.0 / data.NumGames)
						.ToArray())
					.ToArray(),
				NumGamesPlayed = data.NumGames,
				TimeTakenSeconds = stopwatch.Elapsed.TotalSeconds
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
			public Cell[][] Board { get; set; }
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
			public int NumGamesPlayed { get; set; }
			public double TimeTakenSeconds { get; set; }
		}
	}
}
