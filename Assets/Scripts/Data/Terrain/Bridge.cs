using System.Collections.Generic;

public class Bridge
{
    public Island ParentIsland { get; set; }
    public Island DestIsland { get; set; }
    public List<Block> Blocks { get; set; }

    public int Length()
    {
        return Blocks.Count;
    }

    public enum BridgeBlockType
    {
        VERTICAL, HORIZONTAL, CROSS, SIDE_NORTH, SIDE_SOUTH, SIDE_WEST, SIDE_EAST, CORNER_NE, CORNER_SE, CORNER_SW, CORNER_NW,
    }
}