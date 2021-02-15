using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TriggerDialogueBox : MonoBehaviour {
    Image dialogueBox;
    Text text;
    AudioSource audioSource;
	// Use this for initialization
	void Start () {
        dialogueBox = this.GetComponentInChildren<Image>();
        text = this.GetComponentInChildren<Text>();
        audioSource = this.GetComponent<AudioSource>();
        dialogueBox.enabled = false;
        text.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "KIRBY")
        {
            audioSource.Play();
            dialogueBox.enabled = true;
            text.enabled = true;
        }
    }
}
