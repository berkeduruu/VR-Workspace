using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    [Tooltip("Yüklenecek sahnenin adını buraya girin.")]
    public string targetSceneName = "FPS";

    private void OnTriggerEnter(Collider other)
    {
        // Sadece VR Player (oyuncu) girdiğinde tetiklensin
        if (other.transform.root.name.Contains("VR Player") || other.transform.root.CompareTag("Player"))
        {
            // Sadece bir sonraki sahneyi başlatır
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
