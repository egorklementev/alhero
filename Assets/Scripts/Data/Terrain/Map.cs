using UnityEngine;
using System.Collections.Generic;
using BlockType = Block.BlockType;
using Random = UnityEngine.Random;

public class Map 
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public MapParameters Parameters { get; private set; }

    private Block[,] _ground;
    private List<Island> _islands;
    private bool[,] identified;
    private const int MaxTrials = 1024; // If this is exceeded - recreate all level

    public bool IsAir(int x, int y)
    {
        return _ground[x, y].Type <= 0;
    }

    public Block GetBlock(int x, int y)
    {
        return _ground[x, y];
    }

    public bool IsEmptyGroundArea(Vector2Int center, int range = 0)
    {
        for (int x = center.x - range; x <= center.x + range; x++)
        {
            for (int y = center.y - range; y <= center.y + range; y++)
            {
                if (!GetBlock(x, y).IsEmptyGround())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public int NeighborCount(int x, int y)
    {
        int count = 0;
        count += x + 1 < Width && !IsAir(x + 1, y) ? 1 : 0;
        count += x - 1 >= 0 && !IsAir(x - 1, y) ? 1 : 0;
        count += y + 1 < Height && !IsAir(x, y + 1) ? 1 : 0;
        count += y - 1 >= 0 && !IsAir(x, y - 1) ? 1 : 0;
        return count;
    }

    public void Generate(MapParameters prms)
    {
        Parameters = prms;

        Width = prms.dims.x;
        Height = prms.dims.y;

        _ground = new Block[Width, Height];
        _islands = new List<Island>();
        identified = new bool[Width, Height];

        MapGenerator.loadingProgress = 0f;

        GenerateGroundAndAir(prms);
        MapGenerator.loadingProgress = 0.1f;

        ProcessIslands(prms);
        MapGenerator.loadingProgress = 0.2f;

        BuildBridges(_islands[0]);
        PostProcessBridges();
        MapGenerator.loadingProgress = 0.3f;

        GenerateIslandBorders();
        MapGenerator.loadingProgress = 0.4f;

        GenerateTrees(prms);
        MapGenerator.loadingProgress = 0.5f;

        SpawnPortals(prms);
        MapGenerator.loadingProgress = 0.6f;

        SpawnIngredients(prms);
        MapGenerator.loadingProgress = 0.7f;

        SpawnContainers(prms);
        MapGenerator.loadingProgress = 0.8f;

        SpawnEntities(prms);
        MapGenerator.loadingProgress = 0.9f;

        // Here width & height of the map change
        GenerateOutlineBorders();

        MapGenerator.loadingProgress = 0.9f;
    }

    private void GenerateGroundAndAir(MapParameters prms)
    {
        // Generate raw ground & air blocks
        Vector3 np = prms.noiseParameters * prms.scale;
        for (int w = 0; w < Width; w++)
        {
            for (int h = 0; h < Height; h++)
            {
                float value_big = Mathf.PerlinNoise(
                    (float) w / Width * np.x + prms.noiseOffset.x,
                    (float) h / Height * np.x + prms.noiseOffset.y 
                );
                float value_mid = Mathf.PerlinNoise(
                    (float) w / Width * np.y + prms.noiseOffset.x,
                    (float) h / Height * np.y + prms.noiseOffset.y
                );
                float value_small = Mathf.PerlinNoise(
                    (float) w / Width * np.z + prms.noiseOffset.x,
                    (float) h / Height * np.z + prms.noiseOffset.y
                );

                float value = (value_big + value_mid + value_small) / 
                    (np.x + np.y + np.z);
                value = Mathf.Pow(value, prms.exponent);

                if (value > .5f)
                {
                    _ground[w, h] = new Block(BlockType.GROUND, new Vector2Int(w, h));
                }
                else
                {
                    _ground[w, h] = new Block(BlockType.AIR, new Vector2Int(w, h));
                }
            }
        }
    }

    private void ProcessIslands(MapParameters prms)
    {
        // Identify islands
        for (int w = 0; w < Width; w++)
        {
            for (int h = 0; h < Height; h++)
            {
                if (!identified[w, h] && _ground[w, h].Type != 0)
                {
                    _islands.Add(new Island(IdentifyIsland(w, h), this));
                }
            }
        }

        // Eliminate small islands
        foreach (Island i in _islands)
        {
            if (i.Size() < prms.islandSizeThreshold)
            {
                foreach (Block b in i)
                {
                    _ground[b.Location.x, b.Location.y].SetAir();
                }
            }
        }
        _islands.RemoveAll(i => i.IsAllAir());

        // We have to have all islands connected (indirectly)
        _islands.Sort(
            delegate (Island i1, Island i2) 
            {
                return i1.Size() > i2.Size() ? -1 : i1.Size() == i2.Size() ? 0 : 1;
            }
        );
    }

    private void BuildBridges(Island island)
    {
        // -- Add bridges --
        // Start with the largest island
        // We select the shortest bridges possible

        // Mark it as visited 
        island.SetConnected();

        // Try to build bridges from all borders
        int[] stepsX = new int[] { 0, 0, -1, 1};
        int[] stepsY = new int[] { 1, -1, 0, 0};
        Island.BlockFacing[] sides = new Island.BlockFacing[] 
        { 
            Island.BlockFacing.NORTH,  
            Island.BlockFacing.SOUTH,  
            Island.BlockFacing.WEST,  
            Island.BlockFacing.EAST  
        };
        List<Bridge> bridges = new List<Bridge>();
        foreach (Block b in island.GetBorderBlocks())
        {
            List<int> indices = new List<int>() { 0, 1, 2, 3 };
            for (int i = 0; i < 4; i++)
            {
                int random = indices[Random.Range(0, indices.Count)];
                // $"Facing({b.Location.x},{b.Location.y}) => {island.GetBlockFacing(b)}".Log(this);
                if ((island.GetBlockFacing(b) & sides[random]) > 0)
                {
                    Bridge someBridge = TryBuildBridge(b, stepsX[random], stepsY[random]);
                    if (someBridge != null)
                    {
                        someBridge.ParentIsland = island;
                        someBridge.Facing = sides[random];
                        bridges.Add(someBridge);
                    }
                }
                indices.Remove(random);
            }
        }

        // $"Overall bridges for this island: {bridges.Count}".Log(this);

        bridges.Sort(
            delegate (Bridge b1, Bridge b2)
            {
                return b1.Length() > b2.Length() ? 1 : b1.Length() < b2.Length() ? -1 : 0;
            }
        );

        List<Island> builtTo = new List<Island>();
        foreach (Bridge someBridge in bridges)
        {
            // $"Bridge length: {someBridge.Length()}".Log(this);
            if (!builtTo.Contains(someBridge.DestIsland))
            {
                builtTo.Add(someBridge.DestIsland);
                // $"Setting bridge: {someBridge.Facing}, size({someBridge.Length()}), to island({someBridge.DestIsland.Size()})".Log(this);
                foreach (Block b in someBridge.Blocks)
                {
                    b.SetBridge(someBridge.IsVertical());
                }
            }
        }

        foreach (Island i in builtTo)
        {
            BuildBridges(i);
        }
    }

    private void PostProcessBridges()
    {
        foreach (Block b in _ground)
        {
            if (b.IsBridge())
            {
                int neighBridges = 0;
                foreach (Block neigh in GetNeighbors(b))
                {
                    if (neigh.IsBridge()) 
                    {
                        neighBridges++;
                    }
                }
                if (neighBridges > 2)
                {
                    b.SetBridge(false, true);
                }
            }
        }
    }

    private void GenerateIslandBorders()
    {
        // Create island borders 
        foreach (Island i in _islands)
        {
            foreach (Block b in i.GetBorderBlocks())
            {
                foreach (Block neighbor in GetNeighbors(b))
                {
                    if (neighbor.Type == BlockType.AIR)
                    {
                        neighbor.SetIslandBorder();
                    }
                }
            }
        }
    }

    private void SpawnPortals(MapParameters prms)
    {
        foreach (LocationData ld in prms.neighborLocations)
        {
            GetRandomEmptyGroundBlock(1).SetPortal(ld);
        }
    }

    private void GenerateTrees(MapParameters prms)
    {
        // Generate trees
        foreach (Block b in _ground)
        {
            if (b.IsEmptyGround() && !(HasNeighbor(b, BlockType.BRIDGE_V) || HasNeighbor(b, BlockType.BRIDGE_H)))
            {
                if (Random.value > 1f - prms.forestDensity )
                {
                    b.SetTree(Random.Range(0, prms.forestVariety));
                }
            }
        }
    }

    private void SpawnIngredients(MapParameters prms)
    {
        int min = Mathf.Min(prms.ingNumRange.x, prms.ingNumRange.y);
        int max = Mathf.Max(prms.ingNumRange.x, prms.ingNumRange.y);
        int ingNum = Random.Range(min, max);
        while (ingNum > 0)
        {   
            GetRandomEmptyGroundBlock()
                .SetIngredient(DataController.GetWeightedIngredientFromList(
                    new List<int>(
                        new List<string>(prms.ingredients).ConvertAll(ing => ing.Hash())
                        )
                    ).id
                );
            ingNum--;
        }
    }

    private void SpawnContainers(MapParameters prms)
    {
        int min = Mathf.Min(prms.contNumRange.x, prms.contNumRange.y);
        int max = Mathf.Max(prms.contNumRange.x, prms.contNumRange.y);
        int contNum = Random.Range(min, max);
        while (contNum > 0)
        {
            Block b = GetRandomEmptyGroundBlock(1);
            LabContainerItems items = new LabContainerItems();
            items.items = new LabContainerItem[Random.Range(0, 3)];
            for (int i = 0; i < items.items.Length; i++)
            {
                items.items[i] = new LabContainerItem(); 
                items.items[i].id = DataController.GetWeightedIngredientFromList(
                    new List<int>(
                        new List<string>(prms.ingredients).ConvertAll(ing => ing.Hash())
                    )
                ).id;
            }
            b.SetContainer(
                new ContainerData(
                    Random.Range(0, prms.containers.Length),
                    items
                    )
            );
            contNum--;
        }
    }

    private void SpawnEntities(MapParameters prms)
    {
        int entCount = Random.Range(prms.entNumRange.x, prms.entNumRange.y + 1);
        while (entCount-- > 0)
        {
            GetRandomEmptyGroundBlock().SetEntity(prms.entitiesForSpawn[Random.Range(0, prms.entitiesForSpawn.Length)]);
        }
    }

    private void GenerateOutlineBorders()
    {
        // Outline borders
        Width += 2;
        Height += 2;
        Block[,] finalGround = new Block[Width, Height];
        for (int i = 0; i < Width - 2; i++)
        {
            for (int j = 0; j < Height - 2; j++)
            {
                finalGround[i + 1, j + 1] = _ground[i, j];
            }
        }

        for (int h = 0; h < Width; h++)
        {
            finalGround[h, 0] = new Block(BlockType.ISLAND_BORDER, new Vector2Int(h, 0));
            finalGround[h, Height - 1] = new Block(BlockType.ISLAND_BORDER, new Vector2Int(h, Height - 1));
        }
        for (int v = 0; v < Height; v++)
        {
            finalGround[0, v] = new Block(BlockType.ISLAND_BORDER, new Vector2Int(0, v));
            finalGround[Width - 1, v] = new Block(BlockType.ISLAND_BORDER, new Vector2Int(Width - 1, v));
        }
        _ground = finalGround;
    }

    private Bridge TryBuildBridge(Block block, int stepX, int stepY)
    {
        List<Block> bridgeBlocks = new List<Block>();
        Vector2Int pos = new Vector2Int(block.Location.x + stepX, block.Location.y + stepY);
        while (IsValidLocation(pos.x, pos.y) && IsAir(pos.x, pos.y))
        {
            bridgeBlocks.Add(_ground[pos.x, pos.y]);
            pos.x += stepX;
            pos.y += stepY;
        }
        if (IsValidLocation(pos.x, pos.y) && _ground[pos.x, pos.y].Type == BlockType.GROUND)
        {
            Island otherIsland = _islands.Find(i => i.HasBlock(pos.x, pos.y));
            if (otherIsland == null)
            {
                $"What a HELL????!!! => x: {pos.x}, y: {pos.y}".Log(this);
            }
            else if (!otherIsland.IsConnected())
            {
                Bridge b = new Bridge();
                b.Blocks = bridgeBlocks;
                b.DestIsland = otherIsland;
                return b;
            }
        }
        return null;
    }

    private Block[] IdentifyIsland(int w, int h)
    {
        identified[w, h] = true;

        List<Block> blocks = new List<Block>();

        if (_ground[w, h].Type > 0)
        {
            blocks.Add(_ground[w, h]);

            int width = _ground.GetLength(0);
            int height = _ground.GetLength(1);

            if (w + 1 < width && !identified[w + 1, h]) 
            {
                blocks.AddRange(IdentifyIsland(w + 1, h));
            }
            if (w - 1 >= 0 && !identified[w - 1, h])
            {
                blocks.AddRange(IdentifyIsland(w - 1, h));
            }
            if (h + 1 < height && !identified[w, h + 1])
            {
                blocks.AddRange(IdentifyIsland(w, h + 1));
            }
            if (h - 1 >= 0 && !identified[w, h - 1])
            {
                blocks.AddRange(IdentifyIsland(w, h - 1));
            }
        }

        return blocks.ToArray();
    }

    private bool IsValidLocation(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    private bool HasNeighbor(Block block, BlockType type)
    {
        int x = block.Location.x;
        int y = block.Location.y;
        if (x - 1 >= 0 && _ground[x - 1, y].Type == type)
        {
            return true;
        }
        if (x + 1 < Width && _ground[x + 1, y].Type == type)
        {
            return true;
        }
        if (y - 1 >= 0 && _ground[x, y - 1].Type == type)
        {
            return true;
        }
        if (y + 1 < Height && _ground[x, y + 1].Type == type)
        {
            return true;
        }
        return false;
    }

    private List<Block> GetNeighbors(Block b)
    {
        List<Block> neighbors = new List<Block>();
        Vector2Int[] coords = new Vector2Int[]
        {
            new Vector2Int(b.Location.x + 1, b.Location.y),
            new Vector2Int(b.Location.x - 1, b.Location.y),
            new Vector2Int(b.Location.x, b.Location.y + 1),
            new Vector2Int(b.Location.x, b.Location.y - 1)
        };
        foreach (Vector2Int v in coords)
        {
            if (IsValidLocation(v.x, v.y))
            {
                neighbors.Add(GetBlock(v.x, v.y));
            }
        }
        return neighbors;
    }

    private Block GetRandomEmptyGroundBlock(int range = 0)
    {
        int x = Random.Range(0, Width);
        int y = Random.Range(0, Height);
        Block b = GetBlock(x, y);
        int trials = 0;
        while (!IsEmptyGroundArea(b.Location, range))
        {
            x = Random.Range(0, Width);               
            y = Random.Range(0, Height);
            b = GetBlock(x, y);

            trials++;
            if (trials > MaxTrials)
            {
                throw new UnityException($"Free ground block not found with range {range}!!!");
            }
        }
        return b;
    }
    private class Bridge
    {
        public Island ParentIsland { get; set; }
        public Island DestIsland { get; set; }
        public List<Block> Blocks { get; set; }
        public Island.BlockFacing Facing { get; set; } 

        public int Length()
        {
            return Blocks.Count;
        }

        public bool IsVertical()
        {
            return (Facing & Island.BlockFacing.NORTH) > 0 || (Facing & Island.BlockFacing.SOUTH) > 0;
        }
    }
}