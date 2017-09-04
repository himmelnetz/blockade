using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace blockade
{
	public class MyProfiler
	{
		private readonly Dictionary<string, ProfilerData> _allData;

		public MyProfiler()
		{
			this._allData = new Dictionary<string, ProfilerData>();
		}

		public ProfilerStep Step(string identifier)
		{
			if (!this._allData.ContainsKey(identifier))
			{
				this._allData.Add(
					identifier,
					new ProfilerData
					{
						Identifier = identifier,
						Count = 0,
						TotalTime = TimeSpan.Zero
					});
			}

			return new ProfilerStep(this._allData[identifier]);
		}

		public List<ProfilerData> GetAllData()
		{
			return this._allData.Select(kvp => new ProfilerData
				{
					Identifier = kvp.Value.Identifier,
					Count = kvp.Value.Count,
					TotalTime = kvp.Value.TotalTime
				}).ToList();
		}

		public void ClearAllData()
		{
			this._allData.Clear();
		}

		public class ProfilerStep : IDisposable
		{
			private readonly ProfilerData _data;
			private readonly Stopwatch _stopwatch;

			public ProfilerStep(ProfilerData data)
			{
				this._data = data;
				this._stopwatch = new Stopwatch();
				this._stopwatch.Start();
			}

			public void Dispose()
			{
				this._stopwatch.Stop();
				this._data.Count++;
				this._data.TotalTime = this._data.TotalTime + this._stopwatch.Elapsed;
			}
		}

		public class ProfilerData
		{
			public string Identifier { get; set; }
			public int Count { get; set; }
			public TimeSpan TotalTime { get; set; }
		}
	}
}

