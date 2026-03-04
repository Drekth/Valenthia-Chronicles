using UnityEngine;

namespace DialogueAndQuest
{
    public class ConvertationComponent : MonoBehaviour, IInteractable
    {
        [Header("Dialogue Settings")]
        [SerializeField] private string greeting = "Hello!";

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
    }
}
