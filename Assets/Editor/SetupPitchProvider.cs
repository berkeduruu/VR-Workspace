using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VRFPSKit.Locomotion.Turning;

public class SetupPitchProvider
{
    [InitializeOnLoadMethod]
    public static void Setup()
    {
        string path = "Assets/VRFPS Kit/Prefabs/Objects/Player/VR Player.prefab";
        using (var editingScope = new PrefabUtility.EditPrefabContentsScope(path))
        {
            GameObject prefabRoot = editingScope.prefabContentsRoot;
            
            // Fix missing scripts so it can be saved
            int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefabRoot);
            if (count > 0)
            {
                Debug.Log($"Removed {count} missing scripts from VR Player");
            }

            ContinuousPitchProvider pitchProvider = prefabRoot.GetComponent<ContinuousPitchProvider>();
            if (pitchProvider == null)
            {
                pitchProvider = prefabRoot.AddComponent<ContinuousPitchProvider>();
            }

            // Trying to copy from existing ActionBasedContinuousTurnProvider
            var turnProvider = prefabRoot.GetComponent<ActionBasedContinuousTurnProvider>();
            if (turnProvider != null)
            {
                pitchProvider.rightHandTurnAction = turnProvider.rightHandTurnAction;
                pitchProvider.turnSpeed = turnProvider.turnSpeed;
            }

            Debug.Log("Successfully added and configured ContinuousPitchProvider to VR Player");
        }
    }
}
