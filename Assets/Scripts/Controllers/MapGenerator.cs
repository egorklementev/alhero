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

    private Map map;

    private void OnEnable() {
        map.Generate(new MapParameters());
    }

    public void GenerateForestGround()
    {
        foreach (Transform child in environment)
        {
            Destroy(child.gameObject);
        }

        int width = mapSize.x;
        int height = mapSize.y;

        offsetWidth = Random.Range(0, 2 << 20);
        offsetHeight = Random.Range(0, 2 << 20);

        map = new Map();

        map.Generate(
            new MapParameters()
                .SetDims(mapSize)
                .SetAnchor(startPoint)
                .SetNoiseParams(noiseParamters)
                .SetNoiseOffset(new Vector2Int(offsetWidth, offsetHeight))
                .SetScale(scale)
                .SetExponent(exponent)
                .SetIslandSizeThreshold(islandSizeThreshold)
                .SetBlockSize(blockSize)
                .SetForestDiversity(trees.Length)
                .SetForestDensity(treeDensity)
        );

        // Actual prefab instatiation
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
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
                                tree.name = trees[blockData.CntmntID].name + $"({w},{h})";
                                break;
                            case Block.ContainmentType.FLORA:
                                break;
                            default:
                                break;
                        } 
                    }
                }
            }
        }
    }

    private GameObject GenerateRandomTree(Transform block, Block blockData)
    {
        float scaleFactor = .75f + Random.value * .5f;
        float heightOffset = 3f;
        Vector3 randomPos = Random.insideUnitSphere * Random.value * .5f;
        randomPos.y = trees[blockData.CntmntID].transform.position.z + heightOffset * scaleFactor;
        randomPos.y *= blockSize;
        GameObject tree = Instantiate(
            trees[blockData.CntmntID],
            block.position + randomPos,
            Quaternion.Euler(-90f, 0f, 360f * Random.value),
            block
        );
        tree.transform.localScale *= 1f / blockSize * scaleFactor;
        return tree;
    }

}
