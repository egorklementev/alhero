using UnityEngine;

public class BaseAI : MonoBehaviour
{
    public LogicController logic;

    private AIState currentState;

    public AIState GetState()
    {
        return currentState;
    }

    public void SetState(AIState state)
    {
        currentState = state;
    }

    public void Transition(string newState)
    {
        if (!newState.Equals(currentState.Name)) // No need to transition to itself
        {
            // $"New state: {newState}".Log<BaseAI>("Transition", typeof(string));
            currentState = currentState.GetAdjacent(newState);
        }
    }
}
