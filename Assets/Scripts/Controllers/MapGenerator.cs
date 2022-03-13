using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public Transform environment;
    public GameObject[] blockList;
    public GameObject[] trees;

    [Space(15f)]
    [SerializeField] public MapParameters mapParams;

    [Space(10f)]
    public MinimapController minimap;

    [Space(10f)]
    public Slider loadingSlider;

    [Space(15f)]
    public SpawnController spawner;
    public LogicController logic;

    public static float loadingProgress = 0f;

    private Map map;

    private void Start() 
    {
        StartCoroutine(GenerateMap());
    }

    private void Update() {
        if (loadingSlider.IsActive())
        {
            loadingSlider.value = loadingProgress;
        }
    }

    public IEnumerator GenerateMap()
    {
        loadingSlider.gameObject.SetActive(true);

        foreach (Transform child in environment)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in spawner.itemsGroup)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in spawner.containersGroup)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in spawner.entitiesGroup)
        {
            if (!child.CompareTag("Player"))
            {
                Destroy(child.gameObject);
            }
        }

        int width = mapParams.dims.x;
        int height = mapParams.dims.y;

        mapParams.noiseOffset.x = Random.Range(0, 2 << 20);
        mapParams.noiseOffset.y = Random.Range(0, 2 << 20);

        // Hashes of ingredients
        int[] ing_hash = new int[mapParams.ingredients.Length];
        int index = 0;
        foreach (string ing_name in mapParams.ingredients)
        {
            ing_hash[index++] = ing_name.Hash();
        }

        mapParams.forestVariety = trees.Length;

        map = new Map();
        int genAttempt = 1;
        while (true)
        {
            try 
            {
                if (genAttempt < 64)
                {
                    map.Generate(mapParams);
                    break;
                }
                "Map generator broke, have a great day, teleporting back...".Err(this);
                logic.ChangeScene("GameScene");
                break;
            }
            catch
            {
                $"Map could not generate! Attempt {genAttempt++}. Regenerating...".Warn(this);
            }
            yield return null;
        }

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
                        mapParams.anchor + new Vector3(
                            2f * mapParams.blockSize * w, 
                            2f * mapParams.blockSize * pref.transform.position.y, 
                            2f * mapParams.blockSize * h
                        ), 
                        pref.transform.rotation, 
                        environment);
                    block.transform.localScale *= mapParams.blockSize;
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
                                    mapParams.containers[((ContainerData)blockData.Cntmnt).id],
                                    GetBlockSpawnLocation(blockData.Location, .75f),
                                    Quaternion.identity,
                                    spawner.containersGroup.transform
                                    );
                                cont.gameObject.name += $"({w},{h})";
                                foreach (LabContainerItem contItem in ((ContainerData)blockData.Cntmnt).items.items)
                                cont.TryToPutItem(
                                    spawner.SpawnItem<ItemWorld>(
                                        contItem.id,
                                        cont.gameObject
                                    )
                                );
                                break;
                            case Block.ContainmentType.PORTAL:
                                LocationData ld = (LocationData)blockData.Cntmnt;
                                spawner.SpawnPortal(
                                    ld.SceneToLoad,
                                    ld.LabelToShow,
                                    ld.Checkpoint,
                                    ld.Color,
                                    block.transform,
                                    GetBlockSpawnLocation(blockData.Location),
                                    Quaternion.Euler(0f, 210f, 0f)
                                );
                                break;
                            case Block.ContainmentType.ENTITY:
                                BaseAI ai = spawner.SpawnEntity(
                                    (string)blockData.Cntmnt,
                                    GetBlockSpawnLocation(blockData.Location),
                                    Quaternion.identity,
                                    spawner.entitiesGroup
                                    );
                                ai.logic = logic;
                                break;
                            default:
                                break;
                        } 
                    }
                }
            }
        }

        loadingSlider.gameObject.SetActive(false);

        logic.SetRandomPlayerPosition();
    }

    public Vector3 GetBlockSpawnLocation (Vector2Int location, float yOffset = 1.5f)
    {
        return mapParams.anchor + new Vector3(location.x + 1, yOffset, location.y + 1) * mapParams.blockSize * 2f;
    }

    public Vector3 GetRandomBlockSpawnLocation()
    {
        Block block = map.GetBlock(Random.Range(1, map.Width - 1), Random.Range(1, map.Height - 1));
        int iterations = 0;
        while (!block.IsEmptyGround())
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
        randomPos.y *= mapParams.blockSize;
        GameObject tree = Instantiate(
            trees[(int)blockData.Cntmnt],
            block.position + randomPos,
            Quaternion.Euler(-90f, 0f, 360f * Random.value),
            block
        );
        tree.transform.localScale *= 1f / mapParams.blockSize * scaleFactor;
        return tree;
    }

    public void DebugRegenerateMap() 
    {
        StartCoroutine(GenerateMap());
    }

}
