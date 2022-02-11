using UnityEngine;

public class Block
{
    public BlockType Type { get; private set; } = BlockType.AIR;
    public int CntmntID { get; private set; } = -1;
    public ContainmentType CntmntType { get; private set; } = ContainmentType.EMPTY; 

    public Vector2Int Location { get; private set; }

    public Block(BlockType type, Vector2Int location) 
    {
        Type = type;
        Location = location;
    }

    public void SetAir()
    {
        Type = BlockType.AIR;
    }

    public void SetBridge(bool isVertical = true)
    {
        Type = isVertical ? BlockType.BRIDGE_V : BlockType.BRIDGE_H;
    }

    public void SetTree(int treeID)
    {
        CntmntID = treeID;
        CntmntType = ContainmentType.TREE;
    }

    public void SetFlora(int floraID)
    {
        CntmntID = floraID;
        CntmntType = ContainmentType.FLORA;
    }

    public bool IsEmpty()
    {
        return CntmntType == ContainmentType.EMPTY && (Type == BlockType.GROUND || Type == BlockType.AIR);
    }

    public enum ContainmentType 
    {
        EMPTY, TREE, FLORA
    }

    public enum BlockType
    {
        AIR, GROUND, BRIDGE_V, BRIDGE_H
    }

}
