using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    public Transform environment;
    public GameObject[] blockList;
    public GameObject[] trees;
    public float[] treeOffsts;
    public GameObject[] floras;
    public float[] floraOffsts;
    public GameObject[] traps;
    public float[] trapOffsets;
    public GameObject[] presets;
    public float[] presetsOffsets;
    public GameObject[] unlockingKeys;

    [Space(15f)]
    [SerializeField] public MapParameters mapParams;

    [Space(100f)]
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
        GenerateMapAsync();
    }

    private void Update() {
        if (loadingSlider.IsActive())
        {
            loadingSlider.value = loadingProgress;
        }
    }

    public async void GenerateMapAsync()
    {
        logic.TogglePlayerPhysics(false);

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
        mapParams.floraVariety = floras.Length;
        mapParams.trapVariety = traps.Length;
        mapParams.presetVariety = presets.Length;
        mapParams.randomSeed = Random.Range(int.MinValue, int.MaxValue);

        map = new Map();
        int genAttempt = 1;
        while (true)
        {
            $"Map gen attempt {genAttempt}...".Log(this);
            if (genAttempt++ > 4)
            {
                "Map generator broke, have a great day, teleporting back...".Err(this);
                UIController.SpawnSideLine("Map has failed to generate!");
                logic.ChangeScene("GameScene");
            }

            await Task.Run(() => map.Generate(mapParams));
            if (map.IsSuccess)
            {
                logic.SetRandomPlayerPosition();
                break;
            }
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
                                GameObject tree = GenerateRandomGroundObj(trees, treeOffsts, block.transform, blockData);
                                tree.name = trees[(int)blockData.Cntmnt].name + $"({w},{h})";
                                break;
                            case Block.ContainmentType.FLORA:
                                GameObject flora = GenerateRandomGroundObj(floras, floraOffsts, block.transform, blockData);
                                flora.name = floras[(int)blockData.Cntmnt].name + $"({w},{h})";
                                break;
                            case Block.ContainmentType.TRAP:
                                GameObject trap = GenerateRandomGroundObj(traps, trapOffsets, block.transform, blockData);
                                trap.name = traps[(int)blockData.Cntmnt].name + $"({w},{h})";
                                if (trap.TryGetComponent<Trap>(out var trapComponent))
                                {
                                    trapComponent.logic = logic;
                                }
                                break;
                            case Block.ContainmentType.PRESET:
                                GameObject preset = GenerateRandomGroundObj(presets, presetsOffsets, block.transform, blockData);
                                preset.name = presets[(int)blockData.Cntmnt].name + $"({w},{h})";
                                break;
                            case Block.ContainmentType.ITEM:
                                ItemWorld iw = spawner.SpawnItem<ItemWorld>(
                                    (int)blockData.Cntmnt, 
                                    GetBlockSpawnLocation(blockData.Location), 
                                    Quaternion.identity);
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
                                        cont.gameObject.transform
                                    )
                                );

                                // Locked containers
                                float dice = Random.value;
                                if (dice > .97f)
                                {
                                    cont.unlockingKeyId = "key_gold".Hash();
                                    cont.unlockingKeyVisualsPrefab = unlockingKeys[0];
                                    cont.SetLocked(true);
                                }
                                else if (dice > .87f)
                                {
                                    cont.unlockingKeyId = "key_silver".Hash();
                                    cont.unlockingKeyVisualsPrefab = unlockingKeys[1];
                                    cont.SetLocked(true);
                                }
                                else if (dice > .67f)
                                {
                                    cont.unlockingKeyId = "key_broze".Hash();
                                    cont.unlockingKeyVisualsPrefab = unlockingKeys[2];
                                    cont.SetLocked(true);
                                }
                                else
                                {
                                    cont.SetLocked(false);
                                }

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
                                Vector3 randRot = Random.rotation.eulerAngles;
                                randRot -= new Vector3(randRot.x, 0f, randRot.z);
                                AIManager ai = spawner.SpawnEntity(
                                    (string)blockData.Cntmnt,
                                    GetBlockSpawnLocation(blockData.Location),
                                    Quaternion.Euler(randRot));
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

        logic.TogglePlayerPhysics(true);
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
        // $"iterations: {iterations}, block: {block.Location}, {block.Type}".Log(this);
        return GetBlockSpawnLocation(block.Location);
    }

    private GameObject GenerateRandomGroundObj(GameObject[] objs, float[] yOffsets, Transform block, Block blockData)
    {
        float bs = mapParams.blockSize;
        float scaleFactor = .75f + Random.value * .5f;
        Vector3 randomPos = Random.insideUnitSphere * bs * .75f;
        randomPos.y = bs + yOffsets[(int)blockData.Cntmnt] * scaleFactor;
        GameObject obj = Instantiate(
            objs[(int)blockData.Cntmnt],
            Vector3.zero,
            Quaternion.Euler(
                objs[(int)blockData.Cntmnt].transform.rotation.x, 
                360f * Random.value, 
                objs[(int)blockData.Cntmnt].transform.rotation.z),
            block
        );
        obj.transform.localScale *= 1f / bs * scaleFactor;
        obj.transform.position = block.position + randomPos;
        return obj;
    }

    public void DebugRegenerateMap() 
    {
        GenerateMapAsync();
    }

}
