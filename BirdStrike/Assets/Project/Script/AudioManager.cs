using System.Collections;
using System.Collections.Generic;
using Common;
using UnityEngine;

public class AudioManager : SingletonMonoBehaviourBase<AudioManager>
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [System.Serializable]
    public class BGMEntry
    {
        public string name;
        public AudioClip clip;
    }
    [System.Serializable]
    public class SEEntry
    {
        public string name;
        public AudioClip clip;
    }

    [SerializeField] private List<BGMEntry> bgmList = new List<BGMEntry>();
    private Dictionary<string, AudioClip> bgmDict;
    
    [SerializeField] private List<SEEntry> seList = new List<SEEntry>();
    private Dictionary<string, AudioClip> seDict;


    private void Awake()
    {
        bgmDict = new Dictionary<string, AudioClip>();
        foreach (var entry in bgmList)
        {
            if (!bgmDict.ContainsKey(entry.name))
            {
                bgmDict.Add(entry.name, entry.clip);
            }
        }

        SetVolumeBgm(PlayerPrefs.GetFloat("BGM_VOLUME", 0.5f));
        
        
        seDict = new Dictionary<string, AudioClip>();
        foreach (var entry in seList)
        {
            if (!seDict.ContainsKey(entry.name))
            {
                seDict.Add(entry.name, entry.clip);
            }
        }

        SetVolumeSe(PlayerPrefs.GetFloat("SE_VOLUME", 0.5f));
    }

    public void PlayBgm(string name)
    {
        if (bgmDict.ContainsKey(name))
        {
            if (bgmSource.clip == bgmDict[name] && bgmSource.isPlaying) return;

            bgmSource.clip = bgmDict[name];
            bgmSource.loop = true;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning($"BGM '{name}' not found.");
        }
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }

    public void SetVolumeBgm(float volume)
    {
        bgmSource.volume = volume;
        PlayerPrefs.SetFloat("BGM_VOLUME", volume);
    }

    public float GetVolumeBgm() => bgmSource.volume;
    
    
    
    public void PlaySe(string name)
    {
        if (seDict.ContainsKey(name))
        {
            seSource.PlayOneShot(seDict[name]);
        }
        else
        {
            Debug.LogWarning($"SE '{name}' not found.");
        }
    }

    public void SetVolumeSe(float volume)
    {
        seSource.volume = volume;
        PlayerPrefs.SetFloat("SE_VOLUME", volume);
    }

    public float GetVolumeSe() => seSource.volume;
}
