[System.Serializable]
public class History
{
    public HistoryEntry[] list;

    public History(int capacity)
    {
        list = new HistoryEntry[capacity];
    }
}
