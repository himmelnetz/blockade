using System;

namespace blockade
{
	public interface IBlockadeHeuristic
	{
		double EvaluateState(ReadOnlyBlockadeState state, int player);
	}
}

