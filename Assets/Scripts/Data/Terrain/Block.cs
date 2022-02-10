using UnityEngine;

public class Block
{
    public int Type { get; private set; } = 0;

    public Vector2Int Location { get; private set; }

    public Block(int type, Vector2Int location) 
    {
        Type = type;
        Location = location;
    }

    public void SetAir()
    {
        Type = 0;
    }

    public void SetBridge(bool isVertical = true)
    {
        Type = isVertical ? 2 : 3;
    }

}
