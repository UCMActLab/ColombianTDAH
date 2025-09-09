using UnityEngine;

public class AudioManagerResumen : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] private AudioClip checkListSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    private float originalPitch;

    private void Start()
    {
        source = GetComponent<AudioSource>();

        originalPitch = source.pitch;

    }
    public void PlayChecklistSound(bool conseguido)
    {
        if (conseguido)
        {
            source.pitch = originalPitch;
        }
        else
        {
            source.pitch = originalPitch * 0.4f;
        }

        source.PlayOneShot(checkListSound);

    }

    public void PlayWinLoseSound(bool win)
    {
        source.pitch = originalPitch;

        if (win) {
            source.PlayOneShot(winSound);

        }
        else
        {
            source.PlayOneShot(loseSound);

        }


    }
}
