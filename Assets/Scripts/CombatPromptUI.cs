using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class CombatPromptUI : MonoBehaviour
{
    public GameObject uiPanel;
    private GameObject vrPlayer;

    public void ShowUI(GameObject player)
    {
        // Try to get the root VR Player object
        vrPlayer = player.transform.root.gameObject;
        if (uiPanel != null)
        {
            uiPanel.SetActive(true);
        }
    }

    public void HideUI()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }

    public void OnYesClicked()
    {
        if (vrPlayer != null)
        {
            DontDestroyOnLoad(vrPlayer);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        SceneManager.LoadScene("FPS");
    }

    public void OnNoClicked()
    {
        HideUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Clean up duplicate VR Players in the new scene
        GameObject[] allPlayers = FindObjectsOfType<GameObject>().Where(g => g.name.Contains("VR Player")).ToArray();
        foreach (var p in allPlayers)
        {
            if (p != vrPlayer)
            {
                Destroy(p);
            }
        }

        // Teleport to the appropriate spawn point in the FPS scene
        GameObject spawnPoint = GameObject.Find("SpawnPoint");
        if (spawnPoint == null) spawnPoint = GameObject.Find("Player Spawn Point");
        
        if (spawnPoint != null)
        {
            vrPlayer.transform.position = spawnPoint.transform.position;
            vrPlayer.transform.rotation = spawnPoint.transform.rotation;
        }
    }
}
