using UnityEngine;

public class CombatPromptTrigger : MonoBehaviour
{
    public CombatPromptUI promptUI;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Combat Trigger Entered by: " + other.name + " on root: " + other.transform.root.name);
        if (other.transform.root.name.Contains("VR Player") || other.transform.root.CompareTag("Player"))
        {
            if (promptUI != null)
            {
                promptUI.ShowUI(other.transform.root.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.name.Contains("VR Player") || other.transform.root.CompareTag("Player"))
        {
            if (promptUI != null)
            {
                promptUI.HideUI();
            }
        }
    }
}
