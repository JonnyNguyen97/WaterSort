using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Manager")]
    [SerializeField] private AudioClip winSFX;
    [SerializeField] private AudioClip pourLiquidSFX;
    [SerializeField] private AudioClip fullFillSFX;

    public void PlayWinSFX(Vector3 posPlay)
    {
        AudioSource.PlayClipAtPoint(winSFX, posPlay, 1f);
    }

    public void PlayPourLiquidSFX(Vector3 posPlay)
    {
        AudioSource.PlayClipAtPoint(pourLiquidSFX, posPlay, 1f);
    }

    public void PlayFullFilSFX(Vector3 posPlay)
    {
        StartCoroutine(PlayClipCoroutine(posPlay));
    }

    IEnumerator PlayClipCoroutine(Vector3 posPlay)
    {
        yield return new WaitForSeconds(0.6f);
        AudioSource.PlayClipAtPoint(fullFillSFX, posPlay, 1f);
    }

}
