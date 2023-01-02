using UnityEngine;

public class AbstractItem : MonoBehaviour
{
    public int id = 0; // Unique ID for any item (shared between World & UI item versions)
    public string item_name = "none"; // Unique name (may be reset in when ID is created)
    public int arcanumMaterialIndex = 2; // For potions
    public ItemLocation location; // Where this item spawns (optional & required for ingredients)

    public enum ItemLocation
    {
        NONE, FOREST, DESERT, WINTER_FOREST, HEAVEN, HELL,
    }

    public virtual bool IsPotion()
    {
        return false;
    }
}
