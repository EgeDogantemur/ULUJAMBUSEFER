using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public AudioSource musicSource;
    public float defaultVolume = 0.5f;
    public float minVolume = 0f;
    public float maxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource.volume = defaultVolume;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVolume(float volume)
    {
        // Volume değerini 0-1 arasında sınırla
        volume = Mathf.Clamp01(volume);
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        PlayerPrefs.SetInt("MusicMuted", musicSource.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    public float GetCurrentVolume()
    {
        return musicSource.volume;
    }

    public bool IsMuted()
    {
        return musicSource.mute;
    }
} 