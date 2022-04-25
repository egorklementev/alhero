using System.Collections.Generic;
using UnityEngine;

public class SquareWalkingAI : WalkingAI
{
    public int RangeEnhancement = 8;

    private Tile[,] _map;
    private int _iterations;
    private int _mapDim;
    private Vector2Int _startTile;
    private Vector2Int _finishTile;
    private List<Tile> _open = new List<Tile>();
    private List<Tile> _closed = new List<Tile>();

    public override void PrepareAction()
    {
        _walkRoute.Clear();
        //_walkRoute.Add(transform.position);
        _iterations = 1024;
        _mapDim = Mathf.Max((int)Mathf.Abs((transform.position.x - _currentDest.x) / colliderBounds), 
            (int)Mathf.Abs((transform.position.z - _currentDest.z) / colliderBounds)) * 2 + 
            RangeEnhancement + (RangeEnhancement % 2 == 0 ? 1 : 0);
        _mapDim = _mapDim % 2 == 0 ? _mapDim + 1 : _mapDim;
        _startTile = new Vector2Int(_mapDim / 2, _mapDim / 2);
        _finishTile = new Vector2Int
        (
            (int)((_currentDest.x - transform.position.x) / colliderBounds) + _mapDim / 2,
            (int)((_currentDest.z - transform.position.z) / colliderBounds) + _mapDim / 2
        );
        _map = new Tile[_mapDim, _mapDim];
        _open.Clear();
        _closed.Clear();
        for (int x = 0; x < _mapDim; x++)
        {
            for (int y = 0; y < _mapDim; y++)
            {
                _map[x, y] = new Tile
                    (
                        new Vector2Int(x, y),
                        Mathf.Abs(x - _finishTile.x) + Mathf.Abs(y - _finishTile.y)
                    );
            }
        }

        _open.Add(_map[_startTile.x, _startTile.y]);
        while (BuildPath());
        Vector3 lastPoint = transform.position;

        // Backtracking
        Tile tile = _closed.Last();
        _walkRoute.Add(GetPosition(tile));
        int index = 1;
        while (!tile.Pos.Equals(_startTile))
        {
            tile.Visited = true;
            // $"closed tile: {tile.Pos}, G: {tile.G}".Log(this);
            foreach (Tile adj in tile.GetAdjacent())
            {
                // $"adj tile: {adj.Pos}, G: {adj.G}".Log(this);
                if (!adj.Visited && adj.G < tile.G)
                {
                    tile = adj;
                    _walkRoute.Add(GetPosition(tile));
                    if (index > 1)
                    {
                        Debug.DrawLine(_walkRoute[index - 1], _walkRoute[index], Color.red, 5f);
                    }
                    index++;
                    break;
                }
            }
        }
        _walkRoute.Reverse();

        for (int i = 0; i < _closed.Count; i++)
        {
            Vector3 nextPoint = GetPosition(_closed[i]);
            List<Vector3> lst = new List<Vector3>()
            {
                nextPoint + Vector3.forward * (colliderBounds / 2.25f) + Vector3.left * (colliderBounds / 2.25f),
                nextPoint + Vector3.forward * (colliderBounds / 2.25f) + Vector3.right * (colliderBounds / 2.25f),
                nextPoint + Vector3.back * (colliderBounds / 2.25f) + Vector3.right * (colliderBounds / 2.25f),
                nextPoint + Vector3.back * (colliderBounds / 2.25f) + Vector3.left * (colliderBounds / 2.25f)
            };
            for (int j = 0; j < 4; j++)
            {
                Debug.DrawLine(lst[j], lst[(j + 1) % 4], Color.grey, 5f);
            }

            lastPoint = nextPoint;
        }
        // _walkRoute.Add(GetPosition(_closed.Last()));
    }

    private bool BuildPath()
    {
        _iterations--;
        if (_iterations < 0)
        {
            return false;
        }

        if (_open.Count == 0)
        {
            $"No path found!!!".Log(this);
            return false;
        }

        Tile tile = GetMinFTile();
        _open.Remove(tile);
        _closed.Add(tile);

        List<Vector2Int> offsets = new List<Vector2Int>()
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        List<Vector3> dirs = new List<Vector3>()
        {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        for (int i = 0; i < 4; i++)
        {
            try
            {
                CheckAdjacent(tile, offsets[i], dirs[i]);
            }
            catch {}
        }

        if (tile.Pos.Equals(_finishTile))
        {
            return false;
        }

        return true;
    }

    private Tile GetMinFTile()
    {
        // Special case
        bool same = true;
        int f = _open[0].F();
        foreach (Tile t in _open)
        {
            if (t.F() != f)
            {
                same = false;
                break;
            }
        }
        if (same)
        {
            // "Special case reached!!!".Log(this);
            _open.Sort((t1, t2) => t1.G.CompareTo(t2.G));
            // $"Returning: tile {_open.Last().Pos}, G {_open.Last().G}".Log(this);
            return _open.Last();
        }

        _open.Sort((t1, t2) => t1.F().CompareTo(t2.F()));
        return _open[0];
    }

    private void CheckAdjacent(Tile tile, Vector2Int offset, Vector3 direction)
    {
        if (!TestRaycasts(GetPosition(tile), direction, colliderBounds, colliderBounds))
        {
            if (!_closed.Contains(_map[tile.Pos.x + offset.x, tile.Pos.y + offset.y]))
            {
                if (!_open.Contains(_map[tile.Pos.x + offset.x, tile.Pos.y + offset.y]))
                {
                    _map[tile.Pos.x + offset.x, tile.Pos.y + offset.y].G = _map[tile.Pos.x, tile.Pos.y].G + 1;
                    _open.Add(_map[tile.Pos.x + offset.x, tile.Pos.y + offset.y]);
                    tile.AddAdj(_map[tile.Pos.x + offset.x, tile.Pos.y + offset.y]);
                    _map[tile.Pos.x + offset.x, tile.Pos.y + offset.y].AddAdj(tile);
                }
            }
        }
    } 

    private Vector3 GetPosition(Tile tile)
    {
        return transform.position - 
            new Vector3(_startTile.x - tile.Pos.x, 0f, _startTile.y - tile.Pos.y) * colliderBounds;
    }

    private struct Tile 
    {
        public Tile(Vector2Int pos, int _h)
        {
            Pos = pos;
            G = 0;
            H = _h;
            _adjs = new List<Tile>();
            Visited = false;
        }

        public Vector2Int Pos;
        public int G;
        public int H;
        public int F()
        {
            return G + H;
        }
        public void AddAdj(Tile adjacent)
        {
            foreach (Tile t in _adjs)
            {
                if (t.Pos.Equals(adjacent.Pos))
                {
                    return;
                }
            }
            _adjs.Add(adjacent);
        }
        public List<Tile> GetAdjacent()
        {
            return _adjs;
        }
        private List<Tile> _adjs;
        public bool Visited;
    }
}