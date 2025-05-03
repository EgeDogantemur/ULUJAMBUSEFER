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
            
            // Sözlüğü oluştur
            foreach (SoundEffect sound in soundEffects)
            {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;
                sound.source.volume = sound.volume * effectsVolume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;
                
                if (!soundDictionary.ContainsKey(sound.name))
                {
                    soundDictionary.Add(sound.name, sound);
                }
            }
            
            // Öntanımlı sesleri ekle
            AddDefaultSounds();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void AddDefaultSounds()
    {
        AddDefaultSound("CloneCreate", cloneCreateSound);
        AddDefaultSound("CloneRewind", cloneRewindSound);
        AddDefaultSound("ButtonPress", buttonPressSound);
        AddDefaultSound("ObjectPickup", objectPickupSound);
        AddDefaultSound("ObjectDrop", objectDropSound);
        AddDefaultSound("Jump", jumpSound);
        AddDefaultSound("BridgeDestroy", bridgeDestroySound);
    }
    
    private void AddDefaultSound(string name, AudioClip clip)
    {
        if (clip != null && !soundDictionary.ContainsKey(name))
        {
            SoundEffect sound = new SoundEffect
            {
                name = name,
                clip = clip,
                volume = 1f,
                pitch = 1f,
                loop = false,
                source = gameObject.AddComponent<AudioSource>()
            };
            
            sound.source.clip = clip;
            sound.source.volume = effectsVolume;
            sound.source.pitch = 1f;
            sound.source.loop = false;
            
            soundDictionary.Add(name, sound);
            soundEffects.Add(sound);
        }
    }

    public void Play(string name)
    {
        if (effectsMuted) return;
        
        if (soundDictionary.TryGetValue(name, out SoundEffect sound))
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning($"SoundManager: Ses bulunamadı - {name}");
        }
    }

    public void Stop(string name)
    {
        if (soundDictionary.TryGetValue(name, out SoundEffect sound))
        {
            sound.source.Stop();
        }
    }

    public void SetEffectsVolume(float volume)
    {
        effectsVolume = Mathf.Clamp01(volume);
        foreach (var sound in soundEffects)
        {
            sound.source.volume = sound.volume * effectsVolume;
        }
        
        PlayerPrefs.SetFloat("EffectsVolume", effectsVolume);
        PlayerPrefs.Save();
    }

    public void ToggleEffects()
    {
        effectsMuted = !effectsMuted;
        foreach (var sound in soundEffects)
        {
            sound.source.mute = effectsMuted;
        }
        
        PlayerPrefs.SetInt("EffectsMuted", effectsMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    public bool AreEffectsMuted()
    {
        return effectsMuted;
    }
    
    public float GetEffectsVolume()
    {
        return effectsVolume;
    }
} 