using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class WinningPopUp : MonoBehaviour
{
    [Header("Winning Popup Settings")]
    public GameObject winningPopupParent;
    [Tooltip("How often the text blinks (in seconds)")]
    public float blinkInterval = 1f;
    [Tooltip("Whether the popup is currently active")]
    private bool isActive = false;
    private Coroutine blinkCoroutine;
    // Start is called before the first frame update
    void Start()
    {
        isActive = false; // Always reset at start
        // Ensure the popup is off by default
        if (winningPopupParent != null)
        {
            winningPopupParent.SetActive(false);
        }
    }
    // Call this from GameManager when the win condition is met
    public void ShowWinningPopup()
    {
        ActivateWinningPopup();
    }
    /// <summary>
    /// Activates the winning popup and starts blinking
    /// </summary>
    public void ActivateWinningPopup()
    {
        if (isActive || winningPopupParent == null)
            return;
        isActive = true;
        winningPopupParent.SetActive(true);
        // Start blinking
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkText());
    }
    /// <summary>
    /// Deactivates the winning popup and stops blinking
    /// </summary>
    public void DeactivateWinningPopup()
    {
        if (!isActive)
            return;
        isActive = false;
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        if (winningPopupParent != null)
        {
            winningPopupParent.SetActive(false);
        }
    }
    /// <summary>
    /// Coroutine that makes the text blink every blinkInterval seconds
    /// </summary>
    IEnumerator BlinkText()
    {
        while (isActive)
        {
            // Show text
            if (winningPopupParent != null)
            {
                winningPopupParent.SetActive(true);
            }
            yield return new WaitForSeconds(blinkInterval);
            // Hide text
            if (winningPopupParent != null)
            {
                winningPopupParent.SetActive(false);
            }
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
