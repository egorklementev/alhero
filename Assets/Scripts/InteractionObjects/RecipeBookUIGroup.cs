using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeBookUIGroup : MonoBehaviour
{
    public RecipeBook recipeBook;

    private void OnEnable()
    {
        recipeBook.ChangePage(0);
    }
}
