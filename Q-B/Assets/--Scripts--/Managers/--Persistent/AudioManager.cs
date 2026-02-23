using UnityEngine;

public class AudioManager : PersistentSingleton<AudioManager>
{
    public enum EMusicType
    {
        None,
        MainMenu,
        InGame
    }
    
    [Header("Inscribed References")]
    
    [SerializeField] private AudioSource mainMenuMusicSource;
    
    [SerializeField] private AudioSource inGameMusicSource;
    
    [SerializeField] private AudioSource sfxSource;
    
    [field: SerializeField] public AudioClip ButtonClickSfx { get; private set; }
    
    [field: SerializeField] public AudioClip BodyImpactSfx { get; private set; }
    
    [field: SerializeField] public AudioClip ExtendArmsSfx { get; private set; }
    
    [field: SerializeField] public AudioClip RetractArmsSfx { get; private set; }
    
    [field: SerializeField] public AudioClip PackageImpactSfx { get; private set; }
    
    [field: SerializeField] public AudioClip LoseSfx { get; private set; }
    
    [field: SerializeField] public AudioClip WinSfxP1 { get; private set; }
    
    [field: SerializeField] public AudioClip WinSfxP2 { get; private set; }
    
    [field: SerializeField] public AudioClip WinSfxP3 { get; private set; }
    
    [field: SerializeField] public AudioClip WinSfxP4 { get; private set; }
    
    [field: SerializeField] public AudioClip PauseSfx { get; private set; }
    
    public void ChangeMusic(EMusicType musicType)
    {
        switch (musicType)
        {
            case EMusicType.None:
                mainMenuMusicSource.Stop();
                inGameMusicSource.Stop();
                break;
            case EMusicType.MainMenu:
                inGameMusicSource.Stop();
                mainMenuMusicSource.Play();
                break;
            case EMusicType.InGame:
                mainMenuMusicSource.Stop();
                inGameMusicSource.Play();
                break;
        }
    }
    
    public void PlaySfxOneShot(AudioClip clip, float volumeScale = 1f, Vector3 location = default)
    {
        
        if (clip == null)
        {
            Debug.Log("SoundManager: PlaySfxOneShot called with null clip");
            return;
        }
        else if (sfxSource == null)
        {
            Debug.LogWarning("SoundManager: PlaySfxOneShot called with null audioSource");
        }
        else
        {
            sfxSource.transform.position = location;
            
            volumeScale = Mathf.Clamp01(volumeScale);
            
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }
    
    
    protected override void Awake()
    {
        //check inscribed references
        if (mainMenuMusicSource == null ||
            inGameMusicSource == null ||
            sfxSource == null)
        {
            Debug.LogError("SoundManager: Error Checking Inscribed References. Destroying SoundManager.");
            
            Destroy(gameObject);
            
            return;
        }
        
        base.Awake();
    }

    private void Start()
    {
        ChangeMusic(EMusicType.MainMenu);
    }
}
