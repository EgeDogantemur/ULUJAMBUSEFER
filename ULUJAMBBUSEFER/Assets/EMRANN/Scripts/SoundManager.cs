using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }

    public List<SoundEffect> soundEffects = new List<SoundEffect>();
    
    public AudioClip cloneCreateSound;
    public AudioClip cloneRewindSound;
    public AudioClip buttonPressSound;
    public AudioClip objectPickupSound;
    public AudioClip objectDropSound;
    public AudioClip jumpSound;
    public AudioClip bridgeDestroySound;
    public AudioClip bridgeRespawnSound;
    
    [Range(0f, 1f)]
    public float effectsVolume = 0.5f;
    private bool effectsMuted = false;

    private Dictionary<string, SoundEffect> soundDictionary = new Dictionary<string, SoundEffect>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSounds()
    {
        // Temel ses efektlerini oluştur
        CreateBasicSoundEffect("CloneCreate", cloneCreateSound);
        CreateBasicSoundEffect("CloneRewind", cloneRewindSound);
        CreateBasicSoundEffect("ButtonPress", buttonPressSound);
        CreateBasicSoundEffect("ObjectPickup", objectPickupSound);
        CreateBasicSoundEffect("ObjectDrop", objectDropSound);
        CreateBasicSoundEffect("Jump", jumpSound);
        CreateBasicSoundEffect("BridgeDestroy", bridgeDestroySound);
        CreateBasicSoundEffect("BridgeRespawn", bridgeRespawnSound);

        // Diğer ses efektlerini ekle
        foreach (SoundEffect sound in soundEffects)
        {
            if (!string.IsNullOrEmpty(sound.name) && sound.clip != null)
            {
                GameObject soundObject = new GameObject($"Sound_{sound.name}");
                soundObject.transform.SetParent(transform);
                
                AudioSource source = soundObject.AddComponent<AudioSource>();
                source.clip = sound.clip;
                source.volume = sound.volume * effectsVolume;
                source.pitch = sound.pitch;
                source.loop = sound.loop;
                source.playOnAwake = false;
                
                sound.source = source;
                
                if (!soundDictionary.ContainsKey(sound.name))
                {
                    soundDictionary.Add(sound.name, sound);
                }
            }
        }
    }

    private void CreateBasicSoundEffect(string name, AudioClip clip)
    {
        if (clip != null && !soundDictionary.ContainsKey(name))
        {
            GameObject soundObject = new GameObject($"Sound_{name}");
            soundObject.transform.SetParent(transform);
            
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = effectsVolume;
            source.playOnAwake = false;
            
            SoundEffect sound = new SoundEffect
            {
                name = name,
                clip = clip,
                source = source
            };
            
            soundDictionary.Add(name, sound);
        }
    }

    public void Play(string name)
    {
        if (!effectsMuted && soundDictionary.TryGetValue(name, out SoundEffect sound))
        {
            if (sound.source != null)
            {
                sound.source.Play();
            }
        }
    }

    public void Stop(string name)
    {
        if (soundDictionary.TryGetValue(name, out SoundEffect sound))
        {
            if (sound.source != null)
            {
                sound.source.Stop();
            }
        }
    }

    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
        foreach (var sound in soundDictionary.Values)
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * effectsVolume;
            }
        }
        
        PlayerPrefs.SetFloat("EffectsVolume", effectsVolume);
        PlayerPrefs.Save();
    }

    public void ToggleEffects()
    {
        effectsMuted = !effectsMuted;
        foreach (var sound in soundDictionary.Values)
        {
            if (sound.source != null)
            {
                sound.source.mute = effectsMuted;
            }
        }
        
        PlayerPrefs.SetInt("EffectsMuted", effectsMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public bool IsEffectsMuted()
    {
        return effectsMuted;
    }
    
    public float GetEffectsVolume()
    {
        return effectsVolume;
    }
} 