[System.Serializable]
public class Block
{
    public int Type { get; private set; } = 0;

    public Block(int type) => Type = type;
}
