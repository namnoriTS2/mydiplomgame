using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ZombieAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    [Header("Idle")]
    [SerializeField] private AudioClip[] idleSounds;
    [SerializeField] private float idleInterval = 5f;

    [Header("Movement")]
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private AudioClip[] runSounds;
    [SerializeField] private float walkStepInterval = 0.6f;
    [SerializeField] private float runStepInterval = 0.4f;

    [Header("Combat")]
    [SerializeField] private AudioClip[] attackSounds;
    [SerializeField] private AudioClip[] hitSounds;
    [SerializeField] private AudioClip[] deathSounds;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float movementVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float idleVolume = 0.3f;

    [Range(0f, 1f)]
    [SerializeField] private float combatVolume = 0.6f;

    private float idleTimer;
    private float stepTimer;
    private bool isDead;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void Update()
    {
        if (isDead)
            return;

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleInterval)
        {
            idleTimer = 0f;
            PlayRandom(idleSounds, idleVolume);
        }
    }

    public void PlayMovementSound(bool isRunning)
    {
        if (isDead)
            return;

        stepTimer += Time.deltaTime;

        float interval = isRunning
            ? runStepInterval
            : walkStepInterval;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;

            AudioClip[] clips = isRunning ? runSounds : walkSounds;

            PlayRandom(clips, movementVolume);
        }
    }

    public void StopMovementSound()
    {
        stepTimer = 0f;
    }

    public void PlayAttackSound()
    {
        if (!isDead)
            PlayRandom(attackSounds, combatVolume);
    }

    public void PlayHitSound()
    {
        if (!isDead)
            PlayRandom(hitSounds, combatVolume);
    }

    public void PlayDeathSound()
    {
        if (isDead)
            return;

        isDead = true;

        audioSource.Stop();

        PlayRandom(deathSounds, combatVolume);
    }

    private void PlayRandom(AudioClip[] clips, float soundVolume)
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];

        if (clip != null)
            audioSource.PlayOneShot(clip, soundVolume);
    }
}