using UnityEngine;
using System.Collections.Generic;
using System;

public class AIState
{
    public string Name { get; private set; } = "unnamed";
    private Dictionary<string, AIState> adjacent = new Dictionary<string, AIState>();

    public AIState(string name) => Name = name;

    public AIState AddAdjState(params AIState[] states)
    {
        if (states != null) {
            foreach (AIState state in states)
            {
                adjacent.Add(state.Name, state);
            }
        }
        return this;
    }

    public AIState GetAdjacent(string stateName)
    {
        AIState adj = this;
        if (!adjacent.ContainsKey(stateName))
        {
            throw new UnityException($"No adjacent AI state with name \"{stateName}\"!!!{Environment.NewLine}Current state: {Name}.");
        }
        adj = adjacent[stateName]; 
        return adj;
    }
}
