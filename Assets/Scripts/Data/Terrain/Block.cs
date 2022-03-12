using UnityEngine;

public class Block
{
    public BlockType Type { get; private set; } = BlockType.AIR;
    public object Cntmnt { get; private set; }
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

    public void SetIslandBorder()
    {
        Type = BlockType.ISLAND_BORDER;
    }

    public void SetTree(int treeID)
    {
        Cntmnt = treeID;
        CntmntType = ContainmentType.TREE;
    }

    public void SetFlora(int floraID)
    {
        Cntmnt = floraID;
        CntmntType = ContainmentType.FLORA;
    }

    public void SetIngredient(int ingID)
    {
        Cntmnt = ingID;
        CntmntType = ContainmentType.INGREDIENT;
    }

    public void SetContainer(ContainerData cd)
    {
        Cntmnt = cd;
        CntmntType = ContainmentType.CONTAINER;
    }

    public void SetPortal(LocationData ld)
    {
        Cntmnt = ld;
        CntmntType = ContainmentType.PORTAL;
    }

    public void SetEntity(string name)
    {
        Cntmnt = name;
        CntmntType = ContainmentType.ENTITY;
    }

    public bool IsEmptyGround()
    {
        return Type == BlockType.GROUND && 
            (CntmntType == ContainmentType.EMPTY || CntmntType == ContainmentType.FLORA);
    }

    public enum ContainmentType 
    {
        EMPTY, TREE, FLORA, INGREDIENT, CONTAINER, ENTITY, PORTAL
    }

    public enum BlockType
    {
        AIR, GROUND, BRIDGE_V, BRIDGE_H, ISLAND_BORDER
    }

    public Color GetMapColor()
    {
        switch (Type) 
        {
            case BlockType.GROUND:
                return new Color(.1f, .7f, .05f);
            case BlockType.BRIDGE_H:
            case BlockType.BRIDGE_V:
                return new Color(.7f, .55f, .05f);
            default:
                return Color.clear;
        }
    }

}
