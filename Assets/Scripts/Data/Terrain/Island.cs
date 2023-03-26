using System;
using System.Collections;
using System.Collections.Generic;

public class Island : IEnumerable {

    private Block[] _blocks;
    private Map _map;
    private bool _isConnected = false; 
    private List<Block> _borderBlocks;
    
    public Island(Block[] blocks, Map map)
    {
        _blocks = blocks;
        _map = map;

        _borderBlocks = new List<Block>();
        foreach (Block b in _blocks)
        {
            int neighbors = _map.NeighborCount(b.Location.x, b.Location.y);
            if (neighbors < 4)
            {
                _borderBlocks.Add(b);
            }
        }

        Bridges = new HashSet<Bridge>();
    }

    public HashSet<Bridge> Bridges { get; }

    public int Size()
    {
        return _blocks.Length;
    }

    public bool IsAllAir()
    {
        return !Array.Exists(_blocks, b => b.Type > 0);
    }

    public List<Block> GetBorderBlocks()
    {
        return _borderBlocks;
    }

    public bool HasBlock(Block b)
    {
        if (b == null)
        {
            return false;
        }
        return Array.IndexOf(_blocks, b) > -1;
    }

    public bool HasBlock(int x, int y)
    {
        return Array.Exists(_blocks, b => b.Location.x == x && b.Location.y == y);
    }

    public Block GetRandomBridgeStartPoint(Random rand)
    {
        var borderBlocks = GetBorderBlocks();
        int trials = borderBlocks.Count;
        var facings = new List<BlockFacing> { BlockFacing.NORTH, BlockFacing.SOUTH, BlockFacing.EAST, BlockFacing.WEST };
        while (trials-- > 0)
        {
            Block randBorderBlock = borderBlocks[rand.Next(0, borderBlocks.Count)];

            var facingsCopy = new List<BlockFacing>(facings);
            for (int i = 0; i < 4; i++)
            {
                var facing = facingsCopy[rand.Next(0, facingsCopy.Count)];
                facingsCopy.Remove(facing);

                if ((GetBlockFacing(randBorderBlock) & facing) > 0)
                    return randBorderBlock;
            }
        }

        return null;
    }

    public BlockFacing GetBlockFacing(Block b)
    {
        int x = b.Location.x;
        int y = b.Location.y;
        BlockFacing facing = BlockFacing.UNDEFINED;
        if (!HasBlock(x + 1, y) && _map.IsValidLocation(x + 1, y))
        {
            facing |= BlockFacing.EAST;
        }
        if (!HasBlock(x - 1, y) && _map.IsValidLocation(x - 1, y))
        {
            facing |= BlockFacing.WEST;
        }
        if (!HasBlock(x, y + 1) && _map.IsValidLocation(x, y + 1))
        {
            facing |= BlockFacing.NORTH;
        }
        if (HasBlock(x, y - 1) && _map.IsValidLocation(x, y - 1))
        {
            facing |= BlockFacing.SOUTH;
        }
        return facing;
    }

    public void SetConnected()
    {
        _isConnected = true;
    }

    public bool IsConnected()
    {
        return _isConnected;
    }

    public IEnumerator GetEnumerator()
    {
        return new BlockNum(_blocks);
    }

    public enum BlockFacing
    {
        UNDEFINED = 0, NORTH = 1, EAST = 2, SOUTH = 4, WEST = 8
    }

    private class BlockNum : IEnumerator
    {
        private int _pos = -1;

        public Block[] _blocks; 

        public BlockNum(Block[] blocks)
        {
            _blocks = blocks;
        }

        public bool MoveNext()
        {
            _pos++;
            return (_pos < _blocks.Length);
        }

        public void Reset()
        {
            _pos = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Block Current
        {
            get 
            {
                try
                {
                    return _blocks[_pos];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}