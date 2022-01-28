using System.Collections.Generic;

[System.Serializable]
public class GameData<T> where T : GameDataEntry
{
    public T[] list;

    public GameData(int capacity)
    {
        list = new T[capacity];
    }

    public Dictionary<int, T> GetDict()
    {
        Dictionary<int, T> dict = new Dictionary<int, T>();
        foreach (T i in list)
        {
            dict.Add(i.id, i);
        }
        return dict;
    }
}