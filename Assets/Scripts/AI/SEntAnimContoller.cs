using UnityEngine;

public class SEntAnimContoller : MonoBehaviour 
{
    [Range(0f, 1f)]
    public float animationChance;
    public float animationTimer;

    public string[] animationClips;

    private Animator anim;
    private float timer;

    private void Awake() {
        anim = GetComponent<Animator>();
        timer = animationTimer;
    }

    private void Update() {
        timer -= Time.deltaTime;

        if (timer < 0f)
        {
            if (DataController.random.Value() < animationChance)
            {
                anim.Play(animationClips[DataController.random.Range(0, animationClips.Length)]);
            } 
            timer = animationTimer;
        }
    }    
}