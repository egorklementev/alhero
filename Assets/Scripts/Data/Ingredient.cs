[System.Serializable]
public class Ingredient
{
    public int id; // Corresponds to World Item id
    public string ing_name; // Localization link
    public float cooldown; // How much of a delay adds this ingredient to the cauldron
    public float breakChance; // How likely is that this ingredient will break the recipe
    public bool hasBeenDiscovered;

    public Potion potionData; // In case this ingredient is potion

    // Used to calculate potion's color
    public float color_r;
    public float color_g;
    public float color_b;
    public float color_a;

    public Ingredient(
        int id,
        string ing_name,
        float cooldown,
        float breakChance,
        float r,
        float g,
        float b,
        float a,
        Potion potionData = null)
    {
        this.id = id;
        this.ing_name = ing_name;
        this.cooldown = cooldown;
        this.breakChance = breakChance;
        color_r = r;
        color_g = g;
        color_b = b;
        color_a = a;
        this.potionData = potionData;
    }
}
