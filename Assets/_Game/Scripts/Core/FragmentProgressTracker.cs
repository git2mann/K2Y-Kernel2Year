using UnityEngine;
using UnityEngine.UI;

public class FragmentProgressTracker : MonoBehaviour
{
    public Image progressBarFill;   // The green fill
    public int currentPoints = 0;
    public int maxPoints = 100;

    void OnEnable()
    {
        DataFragment.OnFragmentCollect += HandleFragmentCollect;
    }

    void OnDisable()
    {
        DataFragment.OnFragmentCollect -= HandleFragmentCollect;
    }

    void HandleFragmentCollect(int amount)
    {
        currentPoints += amount;
        currentPoints = Mathf.Clamp(currentPoints, 0, maxPoints);
        float progress = (float)currentPoints / maxPoints;
        progressBarFill.fillAmount = progress;

        Debug.Log($"Collected {amount}. Total: {currentPoints}");
    }



}
