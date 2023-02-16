using UnityEngine;
using UnityEngine.SceneManagement;

namespace DynamicMeshCutter
{
    public class ReloadDemo : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

}
