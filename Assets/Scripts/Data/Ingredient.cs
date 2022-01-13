[System.Serializable]
public class Ingredient
{
    public string id; // Corresponds to World Item id
    public float cooldown; // How much of a delay adds this ingredient to the cauldron
    public float breakChance; // How likely is that this ingredient will break the recipe

    // Used to calculate potion's color
    public float color_r;
    public float color_g;
    public float color_b;
    public float color_a;

    public Ingredient(
        string id,
        float cooldown,
        float breakChance,
        float r,
        float g,
        float b,
        float a)
    {
        this.id = id;
        this.cooldown = cooldown;
        this.breakChance = breakChance;
        color_r = r;
        color_g = g;
        color_b = b;
        color_a = a;
    }
}
