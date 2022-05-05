using UnityEngine;

public class HeroRespawner : MonoBehaviour 
{
    public LogicController logic;

    public void RespawnPlayer()
    {
        logic.RespawnPlayer();
    }    
}