
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    [SerializeField] private float soundVolume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Сообщаем зомби о звуке в момент начала воспроизведения
        if (audioSource.isPlaying)
        {
            EnemyController.MakeSound(
                transform.position,
                soundVolume
            );
        }
    }

    // Можно вызвать этот метод при выстреле или другом событии
    public void EmitSound()
    {
        EnemyController.MakeSound(
            transform.position,
            soundVolume
        );
    }
}
