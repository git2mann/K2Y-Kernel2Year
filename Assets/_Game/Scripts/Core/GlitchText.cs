using TMPro;
using UnityEngine;
using System.Collections;

public class GlitchText : MonoBehaviour
{
    public TMP_Text textComponent;
    public string baseText = "LOADING...";
    public float glitchInterval = 0.05f;
    public string glitchChars = "#$%@&!?X01";

    private void OnEnable()
    {
        StartCoroutine(GlitchLoop());
    }

    IEnumerator GlitchLoop()
    {
        while (true)
        {
            // Create a glitched version of the text
            string glitched = "";
            for (int i = 0; i < baseText.Length; i++)
            {
                if (Random.value < 0.2f)
                {
                    glitched += glitchChars[Random.Range(0, glitchChars.Length)];
                }
                else
                {
                    glitched += baseText[i];
                }
            }

            textComponent.text = glitched;

            // Small shake
            transform.localPosition = new Vector3(Random.Range(-2f, 2f), Random.Range(-1f, 1f), 0);

            yield return new WaitForSeconds(glitchInterval);
        }
    }
}
