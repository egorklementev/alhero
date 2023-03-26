using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class MinimapController : MonoBehaviour 
{
    public float mapSize = 160f;
    public Renderer mapRenderer;    
    public RectTransform heroMarker;
    public RectTransform oldmanMarker;
    public GameObject portalMarkerPref;
    
    [SerializeField] private GameObject oldman;

    [Space(15f)]
    public LogicController logic;

    private float realMapSize = 10f;
    private Vector2 mapOrigin = Vector2.zero;
    private List<GameObject> portalMarkers = new List<GameObject>();

    private void Update() 
    {
        Vector3 heroPos = logic.GetHeroPosition();
        heroMarker.anchoredPosition = new Vector2(
            ((heroPos.x - mapOrigin.x) / realMapSize) * mapSize + 10f, 
            ((heroPos.z - mapOrigin.y) / realMapSize) * mapSize + 10f);

        if (oldman != null)
        {
            Vector3 oldmanPos = logic.GetOldmanPosition();
            oldmanMarker.anchoredPosition = new Vector2(
                ((oldmanPos.x - mapOrigin.x) / realMapSize) * mapSize + 10f, 
                ((oldmanPos.z - mapOrigin.y) / realMapSize) * mapSize + 10f);
        }
    }

    public void UpdateMinimap(Map map)
    {
        foreach (GameObject pMarker in portalMarkers)
        {
            Destroy(pMarker);
        }

        realMapSize = map.Parameters.blockSize * map.Width * 2f;
        mapOrigin = new Vector2(map.Parameters.anchor.x, map.Parameters.anchor.z);

        Texture2D mapTexture = new Texture2D(map.Width, map.Height);
        for (int w = 0; w < map.Width; w++)
        {
            for (int h = 0; h < map.Height; h++)
            {
                mapTexture.SetPixel(w, h, map.GetBlock(w, h).GetMapColor());

                if (map.GetBlock(w, h).CntmntType == Block.ContainmentType.PORTAL)
                {
                    GameObject marker = Instantiate(portalMarkerPref, transform as RectTransform);
                    (marker.transform as RectTransform).anchoredPosition = new Vector2(
                        ((float)w / map.Width) * mapSize,
                        ((float)h / map.Height) * mapSize
                    );
                    portalMarkers.Add(marker);
                }
            }
        }
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.Apply();
        mapRenderer.material.mainTexture = mapTexture;
        mapRenderer.material.SetColor("_Color", Color.white);
        mapRenderer.material.renderQueue = 2985; // Transparent-15
    }

}