using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSTPlaylistPlayer : MonoBehaviour
{
    public bool randomPlay = false; // checkbox for random play
    public AudioClip[] clips;
    private AudioSource audioSource;
    int clipOrder = 0; // for ordered playlist

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(VerifyPlaying());
    }

    void Update()
    {
        
    }

    IEnumerator VerifyPlaying() {
        while (true) {
            yield return new WaitForSeconds(1f);
            if (!audioSource.isPlaying) {
                if (randomPlay == true) {
                    audioSource.clip = GetRandomClip();
                    audioSource.Play();
                    // if random play is not selected
                }
                else {
                    audioSource.clip = GetNextClip();
                    audioSource.Play();
                }
            }
        }
    }

    // function to get a random clip
    private AudioClip GetRandomClip()
    {
        return clips[Random.Range(0, clips.Length)];
    }

    // function to get the next clip in order, then repeat from the beginning of the list.
    private AudioClip GetNextClip()
    {
        if (clipOrder >= clips.Length - 1)
        {
            clipOrder = 0;
        }
        else
        {
            clipOrder += 1;
        }
        return clips[clipOrder];
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy() {
        StopCoroutine(VerifyPlaying());
    }
}
