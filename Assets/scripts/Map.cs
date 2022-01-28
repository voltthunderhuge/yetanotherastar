using System.Collections;
using System.Collections.Generic;
using UnityEngine;
class Item : System.IComparable<Item>
{
	public float pri;
	public (int x, int z) loc;

	public int CompareTo(Item item)
	{
		return pri.CompareTo(item.pri);
	}
}

public class Map
{
	(int x, int z)[] dirs =
	{
			(-1, 0), (0, 1), (1, 0), (0, -1)
		};

	public Map(Mesh meshTile, Material materialMap)
	{
		_meshTile = meshTile;
		_materialMap = materialMap;

		_tex = materialMap.GetTexture("_MainTex") as Texture2D;
		_w = _tex.width;
		_h = _tex.height;

		_grid = new int[_w * _h];

		_cols = _tex.GetPixels32();
		int idx = 0;
		for (int j=0; j<_h; ++j)
		{
			for (int i=0; i<_w; ++i)
			{
				if (_cols[idx].r > 127)
				{
					_grid[idx] = 1;
				}
				++idx;
			}
		}
	}

	public float Cost((int x, int z) current, (int x, int z) next, (int x, int z) dir, float turnCost)
	{
		(int x, int z) diff = (next.x - current.x, next.z - current.z);
		if (diff.x != dir.x || diff.z != dir.z)
		{
			return turnCost;
		}
		return 1; // change this when we've proved the astar is working properly 
	}

	public int Neighbours((int x, int z) loc, (int x, int z)[] neighbs)
	{
		int count = 0;
		for (int i = 0; i < dirs.Length; ++i)
		{
			(int x, int z) next = (loc.x + dirs[i].x, loc.z + dirs[i].z);

			if (next.x >= 0 && next.x < _w && next.z >= 0 && next.z < _h && (_grid[next.x + (next.z*_w)] == 1))
			{
				neighbs[count] = next;
				++count;
			}
		}
		return count;
	}

	public void Draw()
	{
		/*Vector3 pos = default(Vector3);
		pos.z = 0.5f;
		for (int j=0; j<_h; ++j)
		{
			pos.x = 0.5f;
			for (int i = 0; i < _w; ++i)
			{
				Graphics.DrawMesh(_meshTile, pos, Quaternion.identity, _materialTile, 0);
				pos.x += 1.0f;
			}
			pos.z += 1.0f;
		}*/
		Graphics.DrawMesh(_meshTile, Matrix4x4.TRS(new Vector3(_tex.width / 2, 0.0f, _tex.height / 2), Quaternion.identity, new Vector3(_tex.width, 1, _tex.height)), _materialMap, 0);
	}

	private int _w;
	private int _h;
	private int[] _grid;
	private Mesh _meshTile;
	private Material _materialMap;
	private Color32[] _cols;
	private Texture2D _tex;
}