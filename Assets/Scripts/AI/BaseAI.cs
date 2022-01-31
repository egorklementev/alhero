using UnityEngine;

public class BaseAI : MonoBehaviour
{
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
            currentState = currentState.GetAdjacent(newState);
        }
    }
}
