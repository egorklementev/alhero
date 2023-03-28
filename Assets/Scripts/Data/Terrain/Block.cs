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
            Bridge.BridgeBlockType.CORNER_W => BlockType.BRIDGE_C_W,
            Bridge.BridgeBlockType.CORNER_N => BlockType.BRIDGE_C_N,
            Bridge.BridgeBlockType.CORNER_S => BlockType.BRIDGE_C_S,
            Bridge.BridgeBlockType.CORNER_E => BlockType.BRIDGE_C_E,
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
            (Type >= BlockType.BRIDGE_X  && Type <= BlockType.BRIDGE_C_S);
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
        BRIDGE_C_N, BRIDGE_C_W, BRIDGE_C_E, BRIDGE_C_S,
    }
}
