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

    public bool IsAir()
    {
        return Type == BlockType.AIR;
    }

    public void SetBridge(Bridge.BridgeBlockType type)
    {
        Type = type switch
        {
            Bridge.BridgeBlockType.VERTICAL => BlockType.BRIDGE_V,
            Bridge.BridgeBlockType.HORIZONTAL => BlockType.BRIDGE_H,
            Bridge.BridgeBlockType.CROSS => BlockType.BRIDGE_X,
            Bridge.BridgeBlockType.SIDE_NORTH => BlockType.BRIDGE_S_N,
            Bridge.BridgeBlockType.SIDE_EAST => BlockType.BRIDGE_S_E,
            Bridge.BridgeBlockType.SIDE_SOUTH => BlockType.BRIDGE_S_S,
            Bridge.BridgeBlockType.SIDE_WEST => BlockType.BRIDGE_S_W,
            Bridge.BridgeBlockType.CORNER_NW => BlockType.BRIDGE_C_NW,
            Bridge.BridgeBlockType.CORNER_NE => BlockType.BRIDGE_C_NE,
            Bridge.BridgeBlockType.CORNER_SW => BlockType.BRIDGE_C_SW,
            Bridge.BridgeBlockType.CORNER_SE => BlockType.BRIDGE_C_SE,
            _ => BlockType.AIR
        };
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

    public void SetTrap(int trapID)
    {
        Cntmnt = trapID;
        CntmntType = ContainmentType.TRAP;
    }

    public void SetPreset(int presetID)
    {
        Cntmnt = presetID;
        CntmntType = ContainmentType.PRESET;
    }

    public void SetItem(int itemID)
    {
        Cntmnt = itemID;
        CntmntType = ContainmentType.ITEM;
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

    public bool IsBridge()
    {
        return Type == BlockType.BRIDGE_V || 
            Type == BlockType.BRIDGE_H || 
            Type == BlockType.BRIDGE_X || 
            (Type >= BlockType.BRIDGE_S_N  && Type <= BlockType.BRIDGE_C_SW);
    }

    public enum ContainmentType 
    {
        EMPTY, TREE, FLORA, ITEM, CONTAINER, ENTITY, PORTAL, TRAP, PRESET
    }

    public enum BlockType
    {
        AIR,
        GROUND,
        BRIDGE_V, BRIDGE_H,
        ISLAND_BORDER,
        BRIDGE_X,
        BRIDGE_S_N, BRIDGE_S_E, BRIDGE_S_S, BRIDGE_S_W,
        BRIDGE_C_NE, BRIDGE_C_NW, BRIDGE_C_SE, BRIDGE_C_SW,
    }

    public Color GetMapColor()
    {
        switch (Type) 
        {
            case BlockType.GROUND:
                return new Color(.1f, .7f, .05f, 1f);
            case BlockType.BRIDGE_H:
            case BlockType.BRIDGE_V:
            case BlockType.BRIDGE_X:
                return new Color(.7f, .55f, .05f, 1f);
            default:
                return Color.clear;
        }
    }

}
