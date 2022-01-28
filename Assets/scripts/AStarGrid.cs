using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarGrid
{
	static float heuristic((int x, int z) a, (int x, int z) b)
	{
		return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
	}

	public void Search
		(
		Map map,
		(int x, int z) start,
		(int x, int z) goal,
		Dictionary<(int x, int z), (int x, int z)> came_from,
		Dictionary<(int x, int z), float> g_cost,
		float turnCost)
	{
		came_from.Clear();
		g_cost.Clear();

		// "frontier" is opem list, "came_from" sort of contains the closed list...maybe?
		PriorityQueue<Item> frontier = new PriorityQueue<Item>();

		frontier.Enqueue(new Item() { pri = 0, loc = start });

		// The keys of the came_from map are the reached set and the values of the came_from map are the parent pointers. 
		came_from[start] = start;
		g_cost[start] = 0.0f;

		(int x, int z)[] neighbs = new (int x, int z)[4];
		while (frontier.Count() > 0)
		{
			Item current = frontier.Dequeue();
			(int x, int z) prev = came_from[current.loc];
			if (current.loc == goal)
			{
				break;
			}
			int neighbourCount = map.Neighbours(current.loc, neighbs);
			for (int i = 0; i < neighbourCount; ++i)
			{
				(int x, int z) next = neighbs[i];
				(int x, int z) dir = (current.loc.x - prev.x, current.loc.z - prev.z);
				float newCost = g_cost[current.loc] + map.Cost(current.loc, next, dir, turnCost);
				if (!g_cost.ContainsKey(next) || newCost < g_cost[next])
				{
					g_cost[next] = newCost;
					float priority = newCost + heuristic(next, goal);
					frontier.Enqueue(new Item { loc = next, pri = priority });
					came_from[next] = current.loc;
				}
			}
		}
	}
}

