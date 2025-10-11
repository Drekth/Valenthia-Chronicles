using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sources
{
    public class GameManager : MonoBehaviour
    {
        void Start()
        {
            SceneManager.LoadScene("SC_DevMap", LoadSceneMode.Additive);
        }
    }
}
