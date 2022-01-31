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

		_cablePathMesh = new Mesh();
		_meshFilter = GetComponent<MeshFilter>();
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
						MakeMesh();
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

	private bool Equal((int x, int z) a, (int x, int z) b)
	{
		return a.x == b.x && a.z == b.z;
	}
	private (int, int) Dir((int x, int z) a, (int x, int z) b)
	{
		return (b.x - a.x, b.z - a.z);
	}

	private List<(int, int)> Reduce(Dictionary<(int, int), (int, int)> path, (int x, int z) end, (int x, int z) start)
	{
		List<(int, int)> corners = new List<(int, int)>();

		(int x, int z) a = end;
		(int x, int z) b = path[end];
		(int x, int z) dir = Dir(a, b);

		corners.Add(end);
		while (true)
		{
			a = b;
			b = path[b];

			(int, int) newDir = Dir(a, b);
			if (!Equals(newDir, dir))
			{
				corners.Add(a);
				dir = newDir;
			}

			if (b == start)
			{
				break;
			}
		}
		corners.Add(b);

		corners.Reverse();

		return corners;
	}

	private void MakeMesh()
	{
		List<(int x, int z)> reducedPath = Reduce(_cameFrom, _goal, _start);

		if (reducedPath.Count == 0)
		{
			return;
		}
		int cornerIdx = 0;
		(int x, int z) ta = reducedPath[cornerIdx];
		Vector3 a;
		a.x = Mathf.FloorToInt(ta.x) + 0.5f;
		a.y = 0.0f;
		a.z = Mathf.FloorToInt(ta.z) + 0.5f;

		Vector3[] vtx;
		int[] idx;

		if (reducedPath.Count == 1)
		{
			// special case
			vtx = new Vector3[4];
			idx = new int[4];
			return;
		}

		int vertexCount = (reducedPath.Count - 1) * 4;
		int indexCount = (reducedPath.Count - 1) * 6;
		vtx = new Vector3[vertexCount];
		idx = new int[indexCount];

		(int x, int z) tb;
		Vector3 b;

		Vector3 vdir = default(Vector3);
		int vtxIdx = 0;
		int idxIdx = 0;
		for (cornerIdx=0; cornerIdx<reducedPath.Count-1; ++cornerIdx)
		{
			tb = reducedPath[cornerIdx + 1];
			b.x = Mathf.FloorToInt(tb.x) + 0.5f;
			b.y = 0.0f;
			b.z = Mathf.FloorToInt(tb.z) + 0.5f;

			(int x, int z) dir = Dir(ta, tb);
			vdir.x = dir.x;
			vdir.z = dir.z;
			vdir.Normalize();

			
			int majorAxis = dir.x == 0 ? 2 : 0;
			int minorAxis = majorAxis == 0 ? 2 : 0;
			Debug.Log($"corner {ta} a {a} b {b} maj {majorAxis} min {minorAxis}");

			vtx[vtxIdx][majorAxis] = a[majorAxis] - vdir[majorAxis]*0.5f;
			vtx[vtxIdx][minorAxis] = a[minorAxis] - 0.5f;
		
			vtx[vtxIdx+1][majorAxis] = a[majorAxis] - vdir[majorAxis]*0.5f;
			vtx[vtxIdx+1][minorAxis] = a[minorAxis] + 0.5f;

			vtx[vtxIdx+2][majorAxis] = b[majorAxis] + vdir[majorAxis]*0.5f;
			vtx[vtxIdx+2][minorAxis] = b[minorAxis] - 0.5f;

			vtx[vtxIdx + 3][majorAxis] = b[majorAxis] + vdir[majorAxis]*0.5f;
			vtx[vtxIdx + 3][minorAxis] = b[minorAxis] + 0.5f;

			if ((majorAxis == 0 && Mathf.Sign(vdir[majorAxis]) > 0) ||
				(majorAxis == 2 && Mathf.Sign(vdir[majorAxis]) < 0))
			{
				idx[idxIdx] = vtxIdx;
				idx[idxIdx + 1] = vtxIdx + 1;
				idx[idxIdx + 2] = vtxIdx + 2;
				idx[idxIdx + 3] = vtxIdx + 1;
				idx[idxIdx + 4] = vtxIdx + 3;
				idx[idxIdx + 5] = vtxIdx + 2;
			}
			else
			{
				idx[idxIdx] = vtxIdx;
				idx[idxIdx + 1] = vtxIdx + 2;
				idx[idxIdx + 2] = vtxIdx + 1;
				idx[idxIdx + 3] = vtxIdx + 1;
				idx[idxIdx + 4] = vtxIdx + 2;
				idx[idxIdx + 5] = vtxIdx + 3;
			}
			ta = tb;
			a = b;

			vtxIdx += 4;
			idxIdx += 6;
		}

		/*Debug.Log("Mesh:");
		foreach (var v in vtx)
		{
			Debug.Log("v: " + v);
		}
		foreach (var i in idx)
		{
			Debug.Log("i: " + i);
		}*/

		_cablePathMesh.Clear();
		_cablePathMesh.vertices = vtx;
		_cablePathMesh.triangles = idx;
		_meshFilter.mesh = _cablePathMesh;
	}

	private void DrawPathNodeAt((int x, int z) tile)
	{
		Graphics.DrawMesh(_meshTile, new Vector3(tile.x+0.5f, 0.1f, tile.z+0.5f), Quaternion.identity, _materialPath, 0);
	}

	Dictionary<(int, int), (int, int)> _cameFrom = new Dictionary<(int, int), (int, int)>();
	Dictionary<(int, int), float> g_cost = new Dictionary<(int, int), float>();

	private (int x, int z) _start, _goal;
	private Map _map;

	private MeshFilter _meshFilter;
	private Mesh _cablePathMesh;
}
