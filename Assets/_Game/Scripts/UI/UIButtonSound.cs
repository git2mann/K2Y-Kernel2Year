using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource), typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private AudioSource audioSource;
    private Button button;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayClickSound);
    }

    void PlayClickSound()
    {
        audioSource.Play();
    }
}
