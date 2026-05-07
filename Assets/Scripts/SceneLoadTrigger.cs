using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneLoadTrigger : MonoBehaviour
{
    [Tooltip("Yüklenecek sahnenin adını buraya girin.")]
    public string targetSceneName = "FPS";
    
    private GameObject SceneChangeButton;

    private void OnTriggerEnter(Collider other)
    {
        // Sadece VR Player (oyuncu) girdiğinde tetiklensin
        if (other.transform.root.name.Contains("VR Player") || other.transform.root.CompareTag("Player"))
        {
            SceneChangeButton = other.transform.root.gameObject;
            
            // Karakteri ve silahlarını yeni sahneye aktarmak için korumaya al
            DontDestroyOnLoad(SceneChangeButton);
            
            // Sahne yüklendiğinde yapılacak temizlik işlemleri için abone ol
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Yeni sahneyi yükle
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Yeni sahnede halihazırda bulunan (varsayılan) VR Player'ı bul ve sil ki çakışma olmasın
        GameObject[] allPlayers = FindObjectsOfType<GameObject>().Where(g => g.name.Contains("VR Player")).ToArray();
        foreach (var p in allPlayers)
        {
            if (p != SceneChangeButton)
            {
                Destroy(p);
            }
        }

        // FPS sahnesindeki SpawnPoint (Doğma Noktası) objesini bulup karakteri oraya ışınla
        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint == null) spawnPoint = GameObject.Find("Player Spawn Point");
        
        if (spawnPoint != null)
        {
            SceneChangeButton.transform.position = spawnPoint.transform.position;
            SceneChangeButton.transform.rotation = spawnPoint.transform.rotation;
        }
    }
}
