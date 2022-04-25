using UnityEngine;

public abstract class SomeAI : MonoBehaviour 
{
    public string Name;
    public string[] adjacentStates;

    protected AIManager _aiManager;

    public abstract void PrepareAction();
    public abstract void Act();

    public void SetManager(AIManager manager)
    {
        _aiManager = manager;
    } 
}