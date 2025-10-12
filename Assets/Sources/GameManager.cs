using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sources
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            SceneManager.LoadScene("SC_DevMap", LoadSceneMode.Additive);
        }
        
        //[SerializeField] private WorldManager worldManager;
        //[SerializeField] private GameSession gameSession;
        //[SerializeField] private EventBus eventBus;
        //[SerializeField] private SaveManager saveManager;
    }
}
