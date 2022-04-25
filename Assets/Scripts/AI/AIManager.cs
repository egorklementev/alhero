using System;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public LogicController logic;
    public string DefaultState = "Idle";
    public Animator anim;

    private AIState _currentState;
    private Dictionary<string, SomeAI> _ais = new Dictionary<string, SomeAI>();

    void Start()
    {
        // Create and connect all AI states before everything
        foreach (SomeAI ai in GetComponents<SomeAI>())
        {
            _ais.Add(ai.Name, ai);
            ai.SetManager(this);
        }
        CreateStateTree();
    }

    void FixedUpdate() 
    {
        _ais[_currentState.Name].Act();
    }

    public AIState GetState()
    {
        return _currentState;
    }

    public void SetState(AIState state)
    {
        if (state != null) {
            _currentState = state;
        }
    }

    public void Transition(string newState)
    {
        if (!newState.Equals(_currentState.Name))
        {
            anim.SetBool(_currentState.Name, false);
            _currentState = _currentState.GetAdjacent(newState);
            anim.SetBool(_currentState.Name, true);
            _ais[_currentState.Name].PrepareAction();
        }
    }

    public T GetAI<T>() where T : SomeAI
    {
        if (TryGetComponent<T>(out T ai))
        {
            return ai;
        }
        else
        {
            return null;
        }
    }

    private void CreateStateTree()
    {
        Dictionary<string, AIState> states = new Dictionary<string, AIState>(); 
        foreach (SomeAI ai in _ais.Values)
        {
            states.Add(ai.Name, new AIState(ai.Name));
        }
        foreach (SomeAI ai in _ais.Values)
        {
            foreach (string adj in ai.adjacentStates)
            {
                states[ai.Name].AddAdjState(states[adj]);
            }
        }
        SetState(states[DefaultState]);
    }
}
