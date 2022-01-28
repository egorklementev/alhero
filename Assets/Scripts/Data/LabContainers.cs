using System.Collections.Generic;

[System.Serializable]
public class LabContainers
{
    public LabContainerItems[] list;

    public LabContainers(int capacity)
    {
        list = new LabContainerItems[capacity];
    }

    public Dictionary<int, LabContainerItems> GetDict()
    {
        Dictionary<int, LabContainerItems> dict = new Dictionary<int, LabContainerItems>();
        foreach (LabContainerItems lci in list)
        {
            dict.Add(lci.id, lci);
        }
        return dict;
    }
}
