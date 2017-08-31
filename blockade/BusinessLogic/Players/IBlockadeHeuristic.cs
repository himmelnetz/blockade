using System;

namespace blockade
{
	public interface IBlockadeHeuristic
	{
		double WinScore { get; }
		double LossScore { get; }

		double EvaluateState(ReadOnlyBlockadeState state, int player);
	}
}

