using System.Collections.Generic;
using BlockType = Block.BlockType;
using System.Linq;
using System.Threading.Tasks;
using Random = System.Random;
using UnityEngine;

public partial class Map 
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    public MapParameters Parameters { get; private set; }

    private const int MaxTrials = 128; // If this is exceeded - recreate all level
    private Block[,] _ground;
    private List<Island> _islands;
    private bool[,] identified;
    private Random rand;

    public bool IsAir(int x, int y)
    {
        return _ground[x, y].Type <= 0;
    }

    public bool IsAir(Vector2Int pos)
    {
        return IsAir(pos.x, pos.y);
    }

    public Block GetBlock(Vector2Int pos)
    {
        return GetBlock(pos.x, pos.y);
    }

    public Block GetBlock(int x, int y)
    {
        return _ground[x, y];
    }

    public bool IsEmptyGroundArea(Vector2Int center, int range = 0)
    {
        int xStart = center.x - range < 0 ? 0 : center.x - range;
        int xEnd = center.x + range >= Width ? Width - 1 : center.x + range;
        int yStart = center.y - range < 0 ? 0 : center.y - range;
        int yEnd = center.y + range >= Height ? Height - 1 : center.y + range;
        for (int x = xStart; x <= xEnd; x++)
        {
            for (int y = yStart; y <= yEnd; y++)
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

    public bool Generate(MapParameters prms)
    {
        rand = new Random(prms.randomSeed);

        Parameters = prms;

        Width = prms.dims.x;
        Height = prms.dims.y;

        _ground = new Block[Width, Height];
        _islands = new List<Island>();
        identified = new bool[Width, Height];

        MapGenerator.loadingProgress = 0f;

        GenerateGroundAndAir(prms);
        MapGenerator.loadingProgress = 0.05f;

        ProcessIslands(prms);
        MapGenerator.loadingProgress = 0.1f;

        BuildBridges();
        PostProcessBridges();
        MapGenerator.loadingProgress = 0.6f;

        GenerateIslandBorders();
        MapGenerator.loadingProgress = 0.65f;

        GenerateTrees(prms);
        GenerateFlora(prms);
        GenerateTraps(prms);
        MapGenerator.loadingProgress = 0.7f;

        if (!SpawnPortals(prms))
        {
            MapGenerator.loadingProgress = 1f;
            return false;
        }
        MapGenerator.loadingProgress = 0.75f;

        SpawnIngredients(prms);
        SpawnNonIngredients(prms);
        MapGenerator.loadingProgress = 0.8f;

        SpawnContainers(prms);
        MapGenerator.loadingProgress = 0.85f;

        SpawnEntities(prms);
        MapGenerator.loadingProgress = 0.9f;

        GeneratePresets(prms);
        MapGenerator.loadingProgress = 95f;

        // Here width & height of the map change
        GenerateOutlineBorders();

        MapGenerator.loadingProgress = 1f;
        return true;
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
            (Island i1, Island i2) =>
            {
                return i1.Size() > i2.Size() ? 1 : i1.Size() == i2.Size() ? 0 : -1;
            }
        );
    }

    private Bridge FindShortestBridge(Island i1, Island i2, float loadingIncrement)
    {
        int trials = (int)((i1.GetBorderBlocks().Count + i2.GetBorderBlocks().Count) * .35f);
        Bridge minBridge = null;

        while (trials-- > 0)
        {
            Block b1 = i1.GetRandomBridgeStartPoint(rand);
            Block b2 = i2.GetRandomBridgeStartPoint(rand);

            TryBuildBridge(b1, b2, out var bridge);

            if (bridge == null)
                continue;

            if (minBridge == null || bridge.Length() < minBridge.Length())
            {
                minBridge = bridge;
            }
        }

        MapGenerator.loadingProgress += loadingIncrement;

        return minBridge;
    }

    private void BuildBridges()
    {
        float loadingStart = .1f; // Should be in range defined in the MapGenerator
        float loadingEnd = .6f;

        float loadingWhole = loadingEnd - loadingStart;
        float loadingIncrement = loadingWhole / (_islands.Count * (_islands.Count - 1)) / 2;

        List<Task<Bridge>> bridgeTasks = new List<Task<Bridge>>();
        for (int i = 0; i < _islands.Count; i++)
        {
            for (int j = i + 1; j < _islands.Count; j++)
            {
                bridgeTasks.Add(Task.FromResult(FindShortestBridge(_islands[i], _islands[j], loadingIncrement)));
            }
        }

        Task.WaitAll(bridgeTasks.ToArray());

        var bridges = bridgeTasks.Where(t => t.Result != null).Select(t => t.Result).ToList();
        $"bridges found: {bridges.Count}".Log();
        bridges.Sort((b1, b2) => b1.Length() > b2.Length() ? 1 : b1.Length() == b2.Length() ? 0 : -1);

        while (bridges.Count > 0)
        {
            if (bridges.Count == 0)
                break;

            var bridge = bridges.First();
            bridges.RemoveAt(0);
            
            if (DoesCreateLoop(bridge))
            {
                continue;
            }

            foreach (Block b in bridge.Blocks)
            {
                b.SetBridge(Bridge.BridgeBlockType.CROSS);
            }

            bridge.ParentIsland.Bridges.Add(bridge);
            bridge.DestIsland.Bridges.Add(bridge);
        }
    }

    private bool DoesCreateLoop(Bridge bridge)
    {
        return RetunsToTheOrigin(bridge.DestIsland, bridge.ParentIsland, bridge);
    }

    private bool RetunsToTheOrigin(Island island, Island originIsland, Bridge lastBridge)
    {
        if (island == originIsland)
            return true;

        foreach (var bridge in island.Bridges)
        {
            if (bridge == lastBridge)
                continue;

            if (RetunsToTheOrigin(
                bridge.DestIsland == island ? 
                bridge.ParentIsland : 
                bridge.DestIsland, 
                originIsland,
                bridge))
                return true;
        }

        return false;    
    }

    private void PostProcessBridges()
    {
        foreach (Block b in _ground)
        {
            if (b.IsBridge())
            {
                var neighbors = GetNeighbors(b);

                if (neighbors.All(b => !b.IsAir()))
                {
                    continue;
                }
                else
                {
                    var airBlocks = neighbors.Where(n => n.IsAir()).ToList();

                    if (airBlocks.Count == 2)
                    {
                        var air1 = airBlocks[0];
                        var air2 = airBlocks[1];

                        if (air1.Location.x == air2.Location.x ||
                            air1.Location.y == air2.Location.y)
                        {
                            if (air1.Location.x == air2.Location.x)
                                b.SetBridge(Bridge.BridgeBlockType.HORIZONTAL);
                            else
                                b.SetBridge(Bridge.BridgeBlockType.VERTICAL);
                        }
                        else
                        {
                            if (air1.Location.x < air2.Location.x && air1.Location.y < air2.Location.y)
                                b.SetBridge(Bridge.BridgeBlockType.CORNER_W);
                            else if (air1.Location.x < air2.Location.x && air1.Location.y > air2.Location.y)
                                b.SetBridge(Bridge.BridgeBlockType.CORNER_N);
                            else if (air1.Location.x > air2.Location.x && air1.Location.y > air2.Location.y)
                                b.SetBridge(Bridge.BridgeBlockType.CORNER_S);
                            else
                                b.SetBridge(Bridge.BridgeBlockType.CORNER_E);
                        }
                    }
                    else if (airBlocks.Count == 1)
                    {
                        var air = airBlocks[0];
                        if (air.Location.x != b.Location.x)
                            if (air.Location.x > b.Location.x)
                                b.SetBridge(Bridge.BridgeBlockType.SIDE_WEST);
                            else
                                b.SetBridge(Bridge.BridgeBlockType.SIDE_EAST);
                        else
                            if (air.Location.y > b.Location.y)
                                b.SetBridge(Bridge.BridgeBlockType.SIDE_SOUTH);
                            else
                                b.SetBridge(Bridge.BridgeBlockType.SIDE_NORTH);
                    }
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

    private bool SpawnPortals(MapParameters prms)
    {
        foreach (LocationData ld in prms.neighborLocations)
        {
            float dice = (float)rand.NextDouble();
            if (dice <= ld.appearanceChance && DataController.genData.potionsCooked >= ld.cookedPotionsRequired)
            {
                var block = GetRandomEmptyGroundBlock(1);
                if (block == null)
                {
                    return false;
                }

                block.SetPortal(ld);
            }
        }

        return true;
    }

    private void GenerateTrees(MapParameters prms)
    {
        foreach (Block b in _ground)
        {
            if (b.IsEmptyGround() && !(HasNeighbor(b, BlockType.BRIDGE_V) || HasNeighbor(b, BlockType.BRIDGE_H) || HasNeighbor(b, BlockType.BRIDGE_X)))
            {
                if (rand.NextDouble() > 1f - prms.forestDensity)
                {
                    b.SetTree(rand.Next(0, prms.forestVariety));
                }
            }
        }
    }

    private void GenerateFlora(MapParameters prms)
    {
        foreach (Block b in _ground)
        {
            if (b.IsEmptyGround())
            {
                if (rand.NextDouble() > 1f - prms.floraDensity)
                {
                    b.SetFlora(rand.Next(0, prms.floraVariety));
                }
            }
        }
    }

    private void GenerateTraps(MapParameters prms)
    {
        foreach (Block b in _ground)
        {
            if (b.IsEmptyGround())
            {
                if (rand.NextDouble() > 1f - prms.trapDensity)
                {
                    b.SetTrap(rand.Next(0, prms.trapVariety));
                }
            }
        }
    }

    private void SpawnIngredients(MapParameters prms)
    {
        int min = Mathf.Min(prms.ingNumRange.x, prms.ingNumRange.y);
        int max = Mathf.Max(prms.ingNumRange.x, prms.ingNumRange.y);
        int ingNum = rand.Next(min, max);
        while (ingNum > 0)
        {   
            var block = GetRandomEmptyGroundBlock();
            if (block == null)
            {
                $"Failed to spawn ingredients, {ingNum} left".Warn(this);
                return;
            }

            block.SetItem(DataController.GetWeightedIngredientFromList(
                prms.ingredients.ToList().ConvertAll(ing => ing.Hash()), 
                rand).id);
            ingNum--;
        }
    }

    private void SpawnNonIngredients(MapParameters prms)
    {
        int min = Mathf.Min(prms.nonIngNumRange.x, prms.nonIngNumRange.y);
        int max = Mathf.Max(prms.nonIngNumRange.x, prms.nonIngNumRange.y);
        int nonIngNum = rand.Next(min, max);
        while (nonIngNum > 0)
        {   
            var block = GetRandomEmptyGroundBlock();
            if (block == null)
            {
                $"Failed to spawn non-ingredients, {nonIngNum} left".Warn(this);
                return;
            }

            block.SetItem(DataController.GetWeightedItemFromList(
                prms.nonIngredients.ToList().ConvertAll(item => item.Hash()),
                prms.nonIngredientsRates.ToList(),
                rand));

            nonIngNum--;
        }
    }

    private void SpawnContainers(MapParameters prms)
    {
        int min = Mathf.Min(prms.contNumRange.x, prms.contNumRange.y);
        int max = Mathf.Max(prms.contNumRange.x, prms.contNumRange.y);
        int contNum = rand.Next(min, max);
        while (contNum > 0)
        {
            Block b = GetRandomEmptyGroundBlock(1);
            if (b == null)
            {
                $"Failed to spawn all containers, {contNum} left.".Warn(this);
                return;
            }

            LabContainerItems items = new LabContainerItems();
            items.items = new LabContainerItem[rand.Next(0, 3)];
            for (int i = 0; i < items.items.Length; i++)
            {
                items.items[i] = new LabContainerItem(); 
                items.items[i].id = DataController.GetWeightedIngredientFromList(
                    new List<int>(
                        new List<string>(prms.ingredients).ConvertAll(ing => ing.Hash())
                    ),
                    rand
                ).id;
            }
            b.SetContainer(
                new ContainerData(
                    rand.Next(0, prms.containers.Length),
                    items
                    )
            );
            contNum--;
        }
    }

    private void SpawnEntities(MapParameters prms)
    {
        int entCount = rand.Next(prms.entNumRange.x, prms.entNumRange.y + 1);
        while (entCount-- > 0)
        {
            var block = GetRandomEmptyGroundBlock();
            if (block == null)
            {
                $"Failed to spawn entities, {entCount} left".Warn(this);
                break;
            }

            block.SetEntity(prms.entitiesForSpawn[rand.Next(0, prms.entitiesForSpawn.Length)]);
        }

        // Always one oldman with his cow
        Block oldmanSpawn = GetRandomEmptyGroundBlock(1);
        oldmanSpawn.SetEntity(Parameters.oldmanName);
        GetNeighbors(oldmanSpawn)[0].SetEntity(Parameters.oldmanCowName);

        // Restrict the number of pigeons on the map
        int pigeonCount = rand.Next(0, DataController.genData.maxPigeons - LogicController.ItemsToSpawnInTheLab.Count + 1);
        for (int i = 0; i < pigeonCount; i++)
        {
            Block pigeonSpawn = GetRandomEmptyGroundBlock(1);
            if (pigeonSpawn != null)
            {
                pigeonSpawn.SetEntity(Parameters.pigeonName);
            }
        }
    }

    private void GeneratePresets(MapParameters prms)
    {
        var blocksNum = prms.dims.x * prms.dims.y; 
        int presetNum = (int)(blocksNum * prms.presetDensity);
        while (presetNum-- > 0)
        {
            var id = rand.Next(0, prms.presetVariety);
            GetRandomEmptyGroundBlock(prms.presetBlockSizes[id])?.SetPreset(id);
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

    private class Tile
    {
        public Tile Parent { get; set; }
        public Vector2Int Location { get; set; }
        public int G { get; set; }
        public int H { get; set; }

        public int F => G + H;
    }

    private void TryBuildBridge(Block b1, Block b2, out Bridge bridge)
    {
        bridge = null;

        if (b1 == null || b2 == null)
            return;

        if (b1.Location == b2.Location)
            return;

        int ComputeH(Vector2Int loc1, Vector2Int loc2)
        {
            return (int)(Mathf.Abs(loc1.x - loc2.x) + Mathf.Abs(loc1.y - loc2.y));
        }

        bridge = new Bridge();
        bridge.ParentIsland = _islands.Where(i => i.HasBlock(b1)).FirstOrDefault();
        bridge.DestIsland = _islands.Where(i => i.HasBlock(b2)).FirstOrDefault();

        List<Tile> open = new List<Tile>();
        List<Tile> closed = new List<Tile>();

        var startTile = new Tile
        {
            Parent = null,
            Location = b1.Location,
            G = 0,
            H = ComputeH(b1.Location, b2.Location)
        };

        // Initial tile
        open.Add(startTile);

        while (open.Count > 0 && open.Count < Width * Height)
        {
            // Find a node with the minimum F
            int minF = int.MaxValue;
            Tile minTile = null;
            foreach (Tile t in open)
            {
                if (t.F < minF)
                {
                    minTile = t;
                    minF = t.F;
                }
            }

            if (minTile == null)
            {
                bridge = null;
                return;
            }

            open.Remove(minTile);
            closed.Add(minTile);

            // Find neighboring tiles for "minimum F" tile
            List<Tile> neighbors = new List<Tile>();
            List<Vector2Int> checkDirections = new List<Vector2Int> 
            {
                Vector2Int.down,
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.right,
            };

            foreach (Vector2Int dir in checkDirections)
            {
                var neighLocation = minTile.Location + dir;

                if (!IsValidLocation(neighLocation))
                    continue;

                if (neighLocation == b2.Location)
                {
                    bridge.Blocks = new List<Block>();

                    var finalTile = minTile;
                    bridge.Blocks.Add(GetBlock(finalTile.Location));
                    while (!finalTile.Parent.Equals(startTile))
                    {
                        finalTile = finalTile.Parent;
                        bridge.Blocks.Add(GetBlock(finalTile.Location));
                    }

                    return;
                }

                if (IsAir(neighLocation))
                {
                    neighbors.Add(new Tile
                    {
                        Parent = minTile,
                        Location = neighLocation,
                        G = minTile.G + 1,
                        H = ComputeH(neighLocation, b2.Location),
                    });
                }
            }

            foreach (Tile neighbor in neighbors)
            {
                if (open.Where(t => t.F < neighbor.F || t.Location == neighbor.Location).Any())
                {
                    continue;
                }

                if (closed.Where(t => t.F < neighbor.F || t.Location == neighbor.Location).Any())
                {
                    continue;
                }

                open.Add(neighbor);
            }
        }

        bridge = null;
        return;
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

    public bool IsValidLocation(Vector2Int vec)
    {
        return IsValidLocation(vec.x, vec.y);
    }

    public bool IsValidLocation(int x, int y)
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
        int x = rand.Next(0, Width);
        int y = rand.Next(0, Height);
        Block b = GetBlock(x, y);
        int trials = 0;
        while (!IsEmptyGroundArea(b.Location, range))
        {
            x = rand.Next(0, Width);               
            y = rand.Next(0, Height);
            b = GetBlock(x, y);

            trials++;
            if (trials > MaxTrials)
            {
                return null;
            }
        }
        return b;
    }
}