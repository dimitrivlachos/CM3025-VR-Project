/*
 * 
 * Code by: 
 *      Dimitrios Vlachos
 *      djv1@student.london.ac.uk
 *      dimitri.j.vlachos@gmail.com
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Music")]
    [SerializeField, Tooltip("List of music to cycle through")]
    List<AudioClip> inGameMusic;
    [SerializeField] AudioSource audioSource;

    private int songRotation;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Select random starting song
        int randomSong = Random.Range(0,inGameMusic.Count);
        songRotation = randomSong;

        StartCoroutine(PlayMusic());
    }

    IEnumerator PlayMusic()
    {
        // Get the next clip to play
        AudioClip clip = inGameMusic[songRotation];

        // Play selected song
        //Debug.Log("Now playing... " + clip.name);
        audioSource.clip = clip;
        audioSource.Play();

        // Increment to select the next song
        songRotation++;

        // If we've incremented too far, loop back around
        if (songRotation >= inGameMusic.Count)
        {
            songRotation = 0;
        }

        // Get the length of the selected song
        float songLength = clip.length;
        //Debug.Log("Waiting " + songLength + " until next song...");

        // Wait that long before playing the next one
        yield return new WaitForSeconds(songLength + 1);

        // Play the next song!
        //Debug.Log("Looping back!");
        StartCoroutine(PlayMusic());
    }
}
