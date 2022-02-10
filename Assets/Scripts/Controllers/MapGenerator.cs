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

    [Range(0, 2 << 15)]
    public int offsetWidth;
    [Range(0, 2 << 15)]
    public int offsetHeight;
    [Range(1f, 100f)]
    public float scale = 1f;
    [Range(.01f, 10f)]
    public float exponent = 1f;
    public int islandSizeThreshold = 8;
    public float blockSize = 10f;

    private Map map;

    public void GenerateForestGround()
    {
        foreach (Transform child in environment)
        {
            Destroy(child.gameObject);
        }

        int width = mapSize.x;
        int height = mapSize.y;

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
        );

        // Actual prefab instatiation
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                if (!map.IsAir(w, h))
                {
                    GameObject pref = blockList[map.BlockType(w, h) - 1];
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
                }
            }
        }
    }

}
