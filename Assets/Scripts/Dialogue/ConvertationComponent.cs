using UnityEngine;

public class ConvertationComponent : MonoBehaviour, IInteractable
{
    public void OnInteract()
    {
        if (!CanInteract())
        {
            return;
        }

        if (ConversationUI.Instance != null)
        {
            ConversationUI.Instance.ShowMessage(greeting);
        }
    }

    public bool CanInteract()
    {
        return !string.IsNullOrEmpty(greeting);
    }

    [Header("Dialogue Settings")]
    [SerializeField] private string greeting = "Hello!";
}
