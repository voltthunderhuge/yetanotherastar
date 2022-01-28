using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarTest : MonoBehaviour
{
	public Texture2D	_textureMap;
	public Mesh _meshTile;
	public Material _materialMap;
	public Material _materialPath;
	public Material _materialTileStart;
	public Material _materialTileGoal;

	public float _turnCost = 5.0f;

	public AStarGrid _astar;

    // Start is called before the first frame update
    void Start()
    {
		_map = new Map(_meshTile, _materialMap);
		_start = (0, 0);
		_goal = (_textureMap.width - 1, _textureMap.height - 1);
	}

    // Update is called once per frame
    void Update()
    {
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane p = new Plane(Vector3.up, 0.0f);

		if (p.Raycast(mouseRay, out float hitDistance))
		{
			Vector3 hit = mouseRay.GetPoint(hitDistance);

			if (Input.GetMouseButtonDown(2))
			{
				_start.x = Mathf.FloorToInt(hit.x);
				_start.z = Mathf.FloorToInt(hit.z);
			}
			if (Input.GetMouseButtonDown(0))
			{ 
				_goal.x = Mathf.FloorToInt(hit.x);
				_goal.z = Mathf.FloorToInt(hit.z);
					
				AStarGrid astar = new AStarGrid();

				astar.Search(_map, _start, _goal, _cameFrom, g_cost, _turnCost);

				if (_cameFrom.Count > 0)
				{
					if (_cameFrom.ContainsKey(_goal))
					{
						(int x, int z) path = _cameFrom[_goal];
						int safety = 0;
						while (path != _start && safety++ < 100)
						{
							path = _cameFrom[path];
						}
						if (safety >= 100)
						{
							Debug.Log("FUuuuuuck!");
						}
					}
					else
					{
						Debug.Log("SAD FACE!");
					}
				}
			}
		}

		Draw();
    }

	private void Draw()
	{
		_map.Draw();

		Vector3 pos = new Vector3(0.0f, 0.2f, 0.0f);

		Vector3 start = new Vector3(_start.x + 0.5f, 0.05f, _start.z+0.5f);
		Vector3 goal = new Vector3(_goal.x+0.5f, 0.05f, _goal.z+0.5f);

		Graphics.DrawMesh(_meshTile, start, Quaternion.identity, _materialTileStart, 0);
		Graphics.DrawMesh(_meshTile, goal, Quaternion.identity, _materialTileGoal, 0);

		if (_cameFrom.Count > 0)
		{
			if (_cameFrom.ContainsKey(_goal))
			{
				DrawPathNodeAt(_goal);
				(int x, int z) path = _cameFrom[_goal];
				while (true)
				{
					DrawPathNodeAt(path);
					var prev = _cameFrom[path];
					if (prev == path)
					{
						break;
					}
					path = prev;
				}
				DrawPathNodeAt(path);
			}
		}
	}

	private void DrawPathNodeAt((int x, int z) tile)
	{
		Graphics.DrawMesh(_meshTile, new Vector3(tile.x+0.5f, 0.1f, tile.z+0.5f), Quaternion.identity, _materialPath, 0);
	}

	Dictionary<(int, int), (int, int)> _cameFrom = new Dictionary<(int, int), (int, int)>();
	Dictionary<(int, int), float> g_cost = new Dictionary<(int, int), float>();

	private (int x, int z) _start, _goal;
	private Map _map;
}
