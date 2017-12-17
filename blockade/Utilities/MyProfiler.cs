using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace blockade
{
	public class MyProfiler
	{
		private readonly Dictionary<string, ProfilerTimingData> _allTimingData;
		private readonly Dictionary<string, ProfilerArgumentData> _allArgumentData;

		private readonly Object _timingDataLock;
		private readonly Object _argumentDataLock;

		public MyProfiler()
		{
			this._allTimingData = new Dictionary<string, ProfilerTimingData>();
			this._allArgumentData = new Dictionary<string, ProfilerArgumentData>();
			this._timingDataLock = new Object();
			this._argumentDataLock = new Object();
		}

		public ProfilerStep Step(string identifier)
		{
			lock (this._timingDataLock)
			{
				if (!this._allTimingData.ContainsKey(identifier))
				{
					this._allTimingData[identifier] = new ProfilerTimingData
					{
						Identifier = identifier,
						Count = 0,
						TotalTime = TimeSpan.Zero
					};
				}

				return new ProfilerStep(this._allTimingData[identifier], this._timingDataLock);
			}
		}

		public void RecordArguments(string identifier, string arguments)
		{
			lock (this._argumentDataLock)
			{
				if (!this._allArgumentData.ContainsKey(identifier))
				{
					this._allArgumentData[identifier] = new ProfilerArgumentData
					{
						Identifier = identifier,
						Counts = new Dictionary<string, int>()
					};
				}

				var dictionary = this._allArgumentData[identifier].Counts;
				if (!dictionary.ContainsKey(arguments))
				{
					dictionary[arguments] = 0;
				}
				dictionary[arguments] = dictionary[arguments] + 1;
			}
		}

		public List<ProfilerTimingData> GetAllTimingData()
		{
			lock (this._timingDataLock)
			{
				// intentionally creating new objects to clone data
				return this._allTimingData.Select(kvp => new ProfilerTimingData
					{
						Identifier = kvp.Value.Identifier,
						Count = kvp.Value.Count,
						TotalTime = kvp.Value.TotalTime
					}).ToList();
			}
		}

		public List<ProfilerArgumentCountDistribution> GetArgumentCountDistribution()
		{
			lock (this._argumentDataLock)
			{
				return this._allArgumentData.Select(kvp => new ProfilerArgumentCountDistribution
					{
						Identifier = kvp.Value.Identifier,
						CountDistribution = kvp.Value.Counts
							.GroupBy(kvp2 => kvp2.Value)
								.Select(g => Tuple.Create(g.Key, g.Count()))
								.OrderByDescending(tup => tup.Item2)
								.ToList()
					}).ToList();
			}
		}

		public void ClearAllData()
		{
			lock (this._timingDataLock)
			{
				this._allTimingData.Clear();
			}
			lock (this._argumentDataLock)
			{
				this._allArgumentData.Clear();
			}
		}

		public class ProfilerStep : IDisposable
		{
			private readonly ProfilerTimingData _timingData;
			private readonly Object _timingDataLock;
			private readonly Stopwatch _stopwatch;

			public ProfilerStep(ProfilerTimingData timingData, Object timingDataLock)
			{
				this._timingData = timingData;
				this._timingDataLock = timingDataLock;
				this._stopwatch = new Stopwatch();
				this._stopwatch.Start();
			}

			public void Dispose()
			{
				this._stopwatch.Stop();
				lock (this._timingDataLock)
				{
					this._timingData.Count++;
					this._timingData.TotalTime = this._timingData.TotalTime + this._stopwatch.Elapsed;
				}
			}
		}

		public class ProfilerTimingData
		{
			public string Identifier { get; set; }
			public int Count { get; set; }
			public TimeSpan TotalTime { get; set; }
		}

		private class ProfilerArgumentData
		{
			public string Identifier { get; set; }
			public Dictionary<string, int> Counts;
		}

		public class ProfilerArgumentCountDistribution
		{
			public string Identifier { get; set; }
			public List<Tuple<int, int>> CountDistribution;
		}
	}
}

