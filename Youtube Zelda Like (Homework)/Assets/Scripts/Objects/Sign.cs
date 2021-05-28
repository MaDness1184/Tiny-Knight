using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sign : MonoBehaviour
{
    [Header ("Caches")]
    public GameObject dialogueBox;
    public Text dialogText;
    public Dialogue scriptableObj;

    [Header("Text")]
    public string dialog;
    public bool usingScriptableObj;

    // private
    private bool playerInRange;

    void Update()
    {
        if(Input.GetButtonDown("Interact") && playerInRange) // If the Player is in range and the button "Interact is pressed"
        {
            promptDialogue();
        }
    }

    private void promptDialogue() // Set specific dialogue box active
    {
        if (dialogueBox.activeInHierarchy)
        {
            dialogueBox.SetActive(false);
        }
        else
        {
            if(usingScriptableObj)
            {
                dialogText.text = scriptableObj.GetText();
            }
            else
            {
                dialogText.text = dialog;
            }
            dialogueBox.SetActive(true);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision) // change playerInRange to true if player enters collider
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    public void OnTriggerExit2D(Collider2D collision) // change playerInRange to false if player exits collider
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueBox.SetActive(false);
        }
    }
}