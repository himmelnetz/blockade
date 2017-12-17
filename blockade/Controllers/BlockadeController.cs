using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Autofac;

namespace blockade.Controllers
{
	public class BlockadeController : Controller
	{
		private readonly PlayerProvider _playerProvider;
		private readonly BlockadeGame.Factory _blockadeGameFactory;
		private readonly MyProfiler _myProfiler;

		public BlockadeController(
			PlayerProvider playerProvider,
			BlockadeGame.Factory blockadeGameFactory,
			MyProfiler myProfiler)
		{
			this._playerProvider = playerProvider;
			this._blockadeGameFactory = blockadeGameFactory;
			this._myProfiler = myProfiler;
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
				Board = ConvertMultidimensionalArray(result.Board.To2dArray()),
				ResultsWithFinalTurn = result.FinalOrdering
					.Select(playerI => Tuple.Create(playerI, result.Board.Enumerate((_, cell) => cell)
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
			this._myProfiler.ClearAllData();

			var games = Enumerable.Range(0, data.NumGames)
				.Select(_ => this._blockadeGameFactory(configuration))
				.ToList();

			var results = new List<BlockadeResult>(capacity: data.NumGames);
			var resultLock = new Object();

			ThreadPool.SetMaxThreads(workerThreads: 2, completionPortThreads: 2);
			Parallel.ForEach(games, game =>
			{
				var result = game.Run();
				lock (resultLock)
				{
					results.Add(result);
				}
			});

			stopwatch.Stop();

			return this.Json(new PlayManyGamesResult
			{
				WinPercentages = Enumerable.Range(0, configuration.Players.Count)
					.Select(playerI => Enumerable.Range(0, configuration.Players.Count)
						.Select(resultI => results.Count(r => r.FinalOrdering[resultI] == playerI) * 1.0 / data.NumGames)
						.ToArray())
					.ToArray(),
				NumGamesPlayed = data.NumGames,
				TimeTakenSeconds = stopwatch.Elapsed.TotalSeconds,
				ProfilerTimingData = this._myProfiler.GetAllTimingData(),
				ProfilerArgumentCountDistribution = this._myProfiler.GetArgumentCountDistribution()
			});
		}

		private static T[][] ConvertMultidimensionalArray<T>(T[,] input)
		{
			var rows = input.GetLength(0);
			var cols = input.GetLength(1);
			return Enumerable.Range(0, rows)
				.Select(row => Enumerable.Range(0, cols)
					.Select(col => input[row, col])
					.ToArray())
				.ToArray();
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
			public List<MyProfiler.ProfilerTimingData> ProfilerTimingData { get; set; }
			public List<MyProfiler.ProfilerArgumentCountDistribution> ProfilerArgumentCountDistribution { get; set; }
		}
	}
}
