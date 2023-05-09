using System;
using UnityEngine;

public class SoundPlayer : MonoBehaviour 
{
    [SerializeField] private AIManager _aiManager;
    [SerializeField] private StateToSounds[] soundsMap;
    [SerializeField] private float[] delays;

    private System.Random random;
    private float nextPitch = 1f;

    private void OnEnable() 
    {
        random = new System.Random();
    }

    public void PlayAudio()
    {
        if (_aiManager == null)
        {
            return; // No entity - no sound
        }

        if (_aiManager.GetState() == null)
        {
            return;
        }

        string curState = _aiManager.GetState().Name;
        int index = 0;
        foreach (var stateToSound in soundsMap)
        {
            if (stateToSound.state == curState)
            {
                var randomAudio = stateToSound.audioSources[random.Range(0, stateToSound.audioSources.Length)];
                if (!randomAudio.isPlaying)
                {
                    randomAudio.pitch = nextPitch;
                    randomAudio.PlayDelayed(delays.Length <= index ? 0f : delays[index]);
                    nextPitch = 1f; // Reset pitch
                }
                break;
            }
            
            index++;
        }
    }

    public void PlayAudioRandomPitch()
    {
        if (random != null)
        {
            nextPitch = .85f + .15f * (float) random.NextDouble();
        }
        PlayAudio();
    }

    [Serializable]
    private class StateToSounds 
    {
        public string state;
        public AudioSource[] audioSources;
    }
}