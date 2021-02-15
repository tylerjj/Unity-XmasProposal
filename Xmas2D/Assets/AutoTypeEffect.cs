using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoTypeEffect : MonoBehaviour {
    Text text;
    //speed to wait before showing each new character
    public float delay = 0.1f;
    // this is what's going to be shown when the text has been completely displayed.
    public string fullText1;
    public string fullText2;
    public string fullText3;
    // Our current text always starts empty.
    private string currentText = "";
    private bool hasPlayed;
	// Use this for initialization
	void Start ()
    {
        hasPlayed = false;
        text = GetComponent<Text>();

        fullText1 = fullText1.Replace("NEWLINE", "\n");
        fullText2 = fullText2.Replace("NEWLINE", "\n");
        fullText3 = fullText3.Replace("NEWLINE", "\n");
        // Runs ShowText on initialization.
        if (text.isActiveAndEnabled)
        {
            hasPlayed = true;
            StartCoroutine(ShowText());
        }	
	}
    private void Update()
    {
        if (text.isActiveAndEnabled && !hasPlayed)
        {
            hasPlayed = true;
            StartCoroutine(ShowText());
        }
    }


    IEnumerator ShowText()
    {
        //make a loop that waits for sometime before showing the next character.
        //  Note: This was leaving off the last character, so I changed i < fullText.Length to i <= fullText.Length
        for (int i = 0; i <= fullText1.Length; i++)
        {
            //make currentText = a substring of fullText
            currentText = fullText1.Substring(0, i);
            // sets this object's text component to be the value of our currentText variable.
            this.GetComponent<Text>().text = currentText;
            // waits for 0.1 seconds, then goes to the next character.
            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(5);
        currentText = "";
        for (int j = 0; j <= fullText2.Length; j++)
        {
            currentText = fullText2.Substring(0, j);
            this.GetComponent<Text>().text = currentText;
            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(4);
        currentText = "";
        for (int k = 0; k <= fullText3.Length; k++)
        {
            currentText = fullText3.Substring(0, k);
            this.GetComponent<Text>().text = currentText;
            yield return new WaitForSeconds(delay);
        }
    }
}
