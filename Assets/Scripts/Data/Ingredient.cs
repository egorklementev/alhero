[System.Serializable]
public class Ingredient
{
    public string id; // Corresponds to World Item id
    public float cooldown; // How much of a delay adds this ingredient to the cauldron
    public float breakChance; // How likely is that this ingredient will break the recipe
}
