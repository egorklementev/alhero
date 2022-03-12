[System.Serializable]
public class ContainerData : GameDataEntry
{
    public LabContainerItems items;

    public ContainerData(int id, LabContainerItems items)
    {
        this.id = id;
        this.items = items;
    }
}