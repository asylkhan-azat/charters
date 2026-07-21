using Charters.Sim.Hexes;

namespace Charters.Sim.Movement.Pathfinding;

public static class Pathfinder
{
    [ThreadStatic]
    private static SearchScratch? _threadScratch;

    public static PathfindingResult FindPath(PathfindingParameters parameters)
    {
        var map = parameters.Map;
        var startHex = parameters.StartHex;
        var goalHex = parameters.GoalHex;

        if ((uint)startHex >= (uint)map.Count || (uint)goalHex >= (uint)map.Count)
        {
            return new PathfindingResult(false, ReadOnlyMemory<int>.Empty);
        }

        if (startHex == goalHex)
        {
            return new PathfindingResult(true, ReadOnlyMemory<int>.Empty);
        }

        var scratch = _threadScratch ??= new SearchScratch();
        scratch.Reset(map.Count);
        
        var startAddress = map.AddressOf(startHex);
        var goalAddress = map.AddressOf(goalHex);

        scratch.Costs[startHex] = 0;
        scratch.Open.Enqueue(startHex, (HexAddress.Distance(startAddress, goalAddress), startHex));

        while (scratch.Open.TryDequeue(out var currentHex, out _))
        {
            if (currentHex == goalHex)
            {
                return new PathfindingResult(true, Reconstruct(scratch.Previous, startHex, goalHex));
            }

            if (scratch.Closed[currentHex])
            {
                continue;
            }

            scratch.Closed[currentHex] = true;

            for (var direction = 0; direction < 6; direction++)
            {
                var neighborHex = map.NeighborOf(currentHex, direction);
                if (neighborHex == -1 || scratch.Closed[neighborHex])
                {
                    continue;
                }

                var stepCost = parameters.CostFunction(map[neighborHex]);
                if (stepCost < 0)
                {
                    continue;
                }

                var newCost = scratch.Costs[currentHex] + stepCost;
                if (newCost < scratch.Costs[neighborHex])
                {
                    scratch.Costs[neighborHex] = newCost;
                    scratch.Previous[neighborHex] = currentHex;
                    var heuristic = HexAddress.Distance(map.AddressOf(neighborHex), goalAddress);
                    scratch.Open.Enqueue(neighborHex, (newCost + heuristic, neighborHex));
                }
            }
        }

        return new PathfindingResult(false, ReadOnlyMemory<int>.Empty);
    }

    private static int[] Reconstruct(int[] previous, int startHex, int goalHex)
    {
        var pathLength = 0;
        for (var current = goalHex; current != startHex; current = previous[current])
        {
            if (current == -1) break; // Should not happen if path found
            pathLength++;
        }

        var path = new int[pathLength];
        var pathIndex = pathLength - 1;
        for (var current = goalHex; current != startHex; current = previous[current])
        {
            if (current == -1) break;
            path[pathIndex--] = current;
        }

        return path;
    }

    private sealed class SearchScratch
    {
        public readonly PriorityQueue<int, (int Estimate, int HexIndex)> Open = new();

        public int[] Costs = [];
        public int[] Previous = [];
        public bool[] Closed = [];

        public void Reset(int hexCount)
        {   
            if (Costs.Length < hexCount)
            {
                Costs = new int[hexCount];
                Previous = new int[hexCount];
                Closed = new bool[hexCount];
            }

            Costs.AsSpan(0, hexCount).Fill(int.MaxValue);
            Previous.AsSpan(0, hexCount).Fill(-1);
            Closed.AsSpan(0, hexCount).Clear();
            Open.Clear();
        }
    }
}