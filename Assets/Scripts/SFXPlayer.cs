using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void PlaySFX(AudioClip clip, float minPitch = 1f, float maxPitch = 1f)
    {
        if (clip == null || source == null) return;

        source.pitch = Random.Range(minPitch, maxPitch);
        source.PlayOneShot(clip);
    }
}
