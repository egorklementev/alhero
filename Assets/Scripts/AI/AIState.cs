using System.Collections.Generic;
using UnityEngine;

public class AIState
{
    public string Name { get; private set; } = "unnamed";
    private Dictionary<string, AIState> adjacent = new Dictionary<string, AIState>();

    public AIState(string name) => Name = name;

    public AIState AddAdjState(params AIState[] states)
    {
        foreach (AIState state in states)
        {
            adjacent.Add(state.Name, state);
        }
        return this;
    }

    public AIState GetAdjacent(string stateName)
    {
        AIState adj = this;
        try
        {
           adj = adjacent[stateName]; 
        }
        catch {
            $"No AI state with name \"{stateName}\"!!!".Warn(this, "GetAdjacent", typeof(string));
        }
        return adj;
    }
}
