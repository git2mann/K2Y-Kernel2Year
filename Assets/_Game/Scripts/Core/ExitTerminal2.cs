using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitTerminal2 : MonoBehaviour
{
    public FragmentProgressTracker progressTracker;
    public GameObject lockedPopup;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (progressTracker.currentPoints >= progressTracker.maxPoints)
        {
            Debug.Log("EXIT UNLOCKED. Level complete!");
            SceneManager.LoadScene("LevelThree");
        }
        else
        {
            Debug.Log("EXIT LOCKED. Collect all fragments.");
            if (lockedPopup != null)
            {
                lockedPopup.SetActive(true);
                StartCoroutine(HidePopupAfterSeconds(3f));
            }
        }
    }

    private IEnumerator HidePopupAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lockedPopup != null)
            lockedPopup.SetActive(false);
    }
}
