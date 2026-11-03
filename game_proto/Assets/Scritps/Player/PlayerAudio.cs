using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Walking")]
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private float walkInterval = 0.55f;

    [Header("Running")]
    [SerializeField] private AudioClip[] runSounds;
    [SerializeField] private float runInterval = 0.35f;

    [Header("Jump")]
    [SerializeField] private AudioClip jumpSound;

    [Header("Stealth Kill")]
    [SerializeField] private AudioClip[] stealthKillSounds;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;

    private float stepTimer;
    private AudioClip lastFootstep;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public bool PlayMovementSound(bool isRunning)
    {
        float interval = isRunning ? runInterval : walkInterval;
        AudioClip[] clips = isRunning ? runSounds : walkSounds;

        if (clips == null || clips.Length == 0)
            return false;

        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;

            PlayRandomFootstep(clips);

            return true;
        }

        return false;
    }

    private void PlayRandomFootstep(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clips.Length > 1)
        {
            int attempts = 0;

            while (clip == lastFootstep && attempts < 10)
            {
                clip = clips[Random.Range(0, clips.Length)];
                attempts++;
            }
        }

        lastFootstep = clip;
        PlayClip(clip);
    }

    public void StopMovementSound()
    {
        stepTimer = 0f;
    }

    public void PlayJumpSound()
    {
        PlayClip(jumpSound);
    }

    public void PlayStealthKillSound()
    {
        PlayRandom(stealthKillSounds);
    }

    private void PlayRandom(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        PlayClip(clip);
    }

    private void PlayClip(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, volume);
    }
}