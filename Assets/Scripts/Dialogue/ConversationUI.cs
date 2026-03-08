using TMPro;
using UnityEngine;

public class ConversationUI : MonoBehaviour
{
    public static ConversationUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        HideDialogue();
    }

    public bool IsDialogueActive()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }

    public void ShowMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (dialogueText != null)
        {
            dialogueText.text = message;
        }

        ShowDialogue();
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (playerHUD != null)
        {
            playerHUD.SetActive(true);
        }
    }

    private void ShowDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (playerHUD != null)
        {
            playerHUD.SetActive(false);
        }
    }

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject playerHUD;
    [SerializeField] private TextMeshProUGUI dialogueText;
}
