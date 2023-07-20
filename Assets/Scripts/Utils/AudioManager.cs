using UnityEngine;
using System.Collections;
using DG.Tweening;
using Utils.Singleton;

public class AudioManager : Singleton<AudioManager>
{
    protected AudioManager()
    {
    } // guarantee this will be always a singleton only - can't use the constructor!

    private AudioSource soundsAudioGenerealSource;
    private AudioClip[] generalSounds;

    public static string Sound_general_click = "GenClick";
    public static string Sound_general_fail = "Fail";
    public static string Sound_general_success = "Success";
 
    public void Initialize()
    {
        soundsAudioGenerealSource = gameObject.AddComponent<AudioSource>();
        PrepareGeneralSounds();
    }

    public void PrepareGeneralSounds()
    {
        if (generalSounds == null)
        {
            Debug.Log("Loading general sounds");

            generalSounds = Resources.LoadAll<AudioClip>("General");

            Debug.Log("Number of general sounds loaded: " + generalSounds.Length);
        }
    }
    
    public void PlayGeneralSound(string sound, bool loop = false)
    {
        AudioClip soundClip = GetSound(generalSounds, sound);

        if (soundClip != null)
        {
            soundsAudioGenerealSource.loop = loop;
            soundsAudioGenerealSource.clip = soundClip;
            soundsAudioGenerealSource.volume = 1;
            soundsAudioGenerealSource.Play();

#if UNITY_EDITOR
            Debug.Log("Playing sound: " + sound + ", loop: " + loop);
#endif
        }
    }

    public void StopGeneralSound()
    {
        soundsAudioGenerealSource.Stop();
    }

    private AudioClip GetSound(AudioClip[] list, string sound)
    {
        AudioClip theSound = null;

        if (list != null)
        {
            for (int index = 0; index < list.Length; index++)
            {
                if (list[index].name == sound)
                {
                    theSound = list[index];
                    break;
                }
            }
        }

        return theSound;
    }
}
