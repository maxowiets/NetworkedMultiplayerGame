using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    AudioSource m_AudioSource;
    public List<AudioClip> m_AudioClipList;

    private void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    public void PlayAudio(int index, Vector3 position)
    {
        if (m_AudioSource != null)
        {
            AudioSource.PlayClipAtPoint(m_AudioClipList[index], position);
        }
    }
}