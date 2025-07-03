using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    int progressAmount;
    public Slider progressSlider;
    void Start(){
        progressAmount = 0;
        progressSlider.value = 0;
        DataFragment.OnFragmentCollect += IncreaseProgressAmount;
    }

    void IncreaseProgressAmount(int amount)
    {
        progressAmount += amount;
        progressSlider.value = progressAmount;

        if (progressAmount >= 100)
        {
           //level complete
            Debug.Log("Level Complete!");
        }
    }
    void Update()
    {
       
    }
}
