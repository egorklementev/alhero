using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Vector2Int mapSize;
    public Vector3 startPoint;
    public Vector3 noiseParamters;
    public Transform environment;
    public GameObject[] blockList;
    public GameObject[] trees;
    public int[] containers;
    public Vector2Int containerNumberRange;
    public string[] ingredients;
    public Vector2Int ingredientNumberRange;
    public string[] entities;
    public LocationData[] neighborLocations;
    public MinimapController minimap;

    public int offsetWidth;
    public int offsetHeight;
    [Range(1f, 100f)]
    public float scale = 1f;
    [Range(.01f, 10f)]
    public float exponent = 1f;
    public int islandSizeThreshold = 8;
    public float blockSize = 10f;
    [Range(0f, 1f)]
    public float treeDensity = .2f;

    [Space(15f)]
    public SpawnController spawner;
    public LogicController logic;

    private Map map;

    private void Start() 
    {
        GenerateMap();
        logic.SetRandomPlayerPosition();
    }

    public void GenerateMap()
    {
        foreach (Transform child in environment)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in spawner.itemsGroup.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in spawner.labContainers.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in logic.entitiesGroup.transform)
        {
            if (!child.CompareTag("Player"))
            {
                Destroy(child.gameObject);
            }
        }

        int width = mapSize.x;
        int height = mapSize.y;

        offsetWidth = Random.Range(0, 2 << 20);
        offsetHeight = Random.Range(0, 2 << 20);

        // Hashes of ingredients
        int[] ing_hash = new int[ingredients.Length];
        int index = 0;
        foreach (string ing_name in ingredients)
        {
            ing_hash[index++] = ing_name.Hash();
        }

        MapParameters prms = new MapParameters()
                .SetDims(mapSize)
                .SetAnchor(startPoint)
                .SetNoiseParams(noiseParamters)
                .SetNoiseOffset(new Vector2Int(offsetWidth, offsetHeight))
                .SetScale(scale)
                .SetExponent(exponent)
                .SetIslandSizeThreshold(islandSizeThreshold)
                .SetBlockSize(blockSize)
                .SetForestVariety(trees.Length)
                .SetForestDensity(treeDensity)
                .SetContainerVariety(containers.Length)
                .SetContNumRange(containerNumberRange)
                .AddIngredientForSpawn(ing_hash)
                .SetIngNumRange(ingredientNumberRange)
                .AddEntityForSpawn(entities)
                .SetNeighborLocations(neighborLocations);

        map = new Map();
        map.Generate(prms);

        try 
        {
            minimap.UpdateMinimap(map);
        }
        catch
        {
            $"Minimap update error!!!".Err(this);
        }

        // Actual prefab instatiation
        for (int w = 0; w < map.Width; w++)
        {
            for (int h = 0; h < map.Height; h++)
            {
                if (!map.IsAir(w, h))
                {
                    Block blockData = map.GetBlock(w, h);
                    GameObject pref = blockList[(int)(map.GetBlock(w, h).Type) - 1];
                    GameObject block = Instantiate(
                        pref, 
                        startPoint + new Vector3(
                            2f * blockSize * w, 
                            2f * blockSize * pref.transform.position.y, 
                            2f * blockSize * h
                        ), 
                        pref.transform.rotation, 
                        environment);
                    block.transform.localScale *= blockSize;
                    block.name = pref.name + $"({w},{h})";

                    if (blockData.CntmntType != Block.ContainmentType.EMPTY)
                    {
                        switch (blockData.CntmntType)
                        {
                            case Block.ContainmentType.TREE:
                                GameObject tree = GenerateRandomTree(block.transform, blockData);
                                tree.name = trees[(int)blockData.Cntmnt].name + $"({w},{h})";
                                break;
                            case Block.ContainmentType.FLORA:
                                break;
                            case Block.ContainmentType.INGREDIENT:
                                ItemWorld iw = spawner.SpawnItem<ItemWorld>(
                                    (int)blockData.Cntmnt, 
                                    GetBlockSpawnLocation(blockData.Location), 
                                    Quaternion.identity, 
                                    spawner.itemsGroup
                                    );
                                iw.gameObject.name += $"({w},{h})";
                                break;
                            case Block.ContainmentType.CONTAINER:
                                Container cont = spawner.SpawnContainer(
                                    containers[(int)blockData.Cntmnt],
                                    GetBlockSpawnLocation(blockData.Location, .75f),
                                    Quaternion.identity,
                                    spawner.labContainers.transform
                                    );
                                cont.gameObject.name += $"({w},{h})";
                                for (int i = 0; i < cont.invSize; i++)
                                {
                                    if (Random.value > .9f)
                                    {
                                        cont.TryToPutItem(
                                            spawner.SpawnItem<ItemWorld>(
                                                DataController.GetWeightedIngredientFromList(
                                                    new List<int>(prms.ingredientsForSpawn)
                                                ).id,
                                                spawner.itemsGroup
                                            )
                                        );
                                    }
                                }
                                break;
                            case Block.ContainmentType.PORTAL:
                                LocationData ld = (LocationData)blockData.Cntmnt;
                                spawner.SpawnPortal(
                                    ld.SceneToLoad,
                                    ld.LabelToShow,
                                    ld.Color,
                                    environment,
                                    GetBlockSpawnLocation(blockData.Location, 1.75f),
                                    Quaternion.Euler(0f, 210f, 0f)
                                );
                                break;
                            default:
                                break;
                        } 
                    }
                }
            }
        }
    }

    public Vector3 GetBlockSpawnLocation (Vector2Int location, float yOffset = 1.5f)
    {
        return startPoint + new Vector3(location.x + 1, yOffset, location.y + 1) * blockSize * 2f;
    }

    public Vector3 GetRandomBlockSpawnLocation()
    {
        Block block = map.GetBlock(Random.Range(1, map.Width - 1), Random.Range(1, map.Height - 1));
        int iterations = 0;
        while (!block.IsGroundEmpty())
        {
            block = map.GetBlock(Random.Range(1, map.Width - 1), Random.Range(1, map.Height - 1));
            iterations++;
        }
        $"iterations: {iterations}, block: {block.Location}, {block.Type}".Log(this);
        return GetBlockSpawnLocation(block.Location);
    }

    private GameObject GenerateRandomTree(Transform block, Block blockData)
    {
        float scaleFactor = .75f + Random.value * .5f;
        float heightOffset = 3f;
        Vector3 randomPos = Random.insideUnitSphere * Random.value * .5f;
        randomPos.y = trees[(int)blockData.Cntmnt].transform.position.z * (1f - scaleFactor) + heightOffset;
        randomPos.y *= blockSize;
        GameObject tree = Instantiate(
            trees[(int)blockData.Cntmnt],
            block.position + randomPos,
            Quaternion.Euler(-90f, 0f, 360f * Random.value),
            block
        );
        tree.transform.localScale *= 1f / blockSize * scaleFactor;
        return tree;
    }

}
