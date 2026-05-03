using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace VRFPSKit
{
    public static class XRFilterDebugger
    {
        public static void DebugSelectFilters(
            IXRSelectInteractor interactor,
            IXRSelectInteractable interactable)
        {
            var baseInteractor = interactor as XRBaseInteractor;
            if (baseInteractor == null)
            {
                Debug.LogError("Interactor is not XRBaseInteractor");
                return;
            }

            var manager = baseInteractor.interactionManager;
            if (manager == null)
            {
                Debug.LogError("Interactor has no InteractionManager");
                return;
            }

            Debug.Log("====== XR SELECT FILTER DEBUG ======");

            if (baseInteractor.selectFilters != null && baseInteractor.selectFilters.count > 0)
            {
                Debug.Log("Interactor Select Filters:");
                for (int i = 0; i < baseInteractor.selectFilters.count; i++)
                {
                    var filter = baseInteractor.selectFilters.GetAt(i);
                    if (!filter.canProcess)
                        Debug.Log($"  [{i}] {filter.GetType().Name} → SKIPPED (canProcess = false)");
                    else
                        Debug.Log($"  [{i}] {filter.GetType().Name} → {filter.Process(interactor, interactable)}");
                }
            }
            else
            {
                Debug.Log("Interactor Select Filters: <none>");
            }

            if (interactable is XRBaseInteractable baseInteractable)
            {
                if (baseInteractable.selectFilters != null && baseInteractable.selectFilters.count > 0)
                {
                    Debug.Log("Interactable Select Filters:");
                    for (int i = 0; i < baseInteractable.selectFilters.count; i++)
                    {
                        var filter = baseInteractable.selectFilters.GetAt(i);
                        if (!filter.canProcess)
                            Debug.Log($"  [{i}] {filter.GetType().Name} → SKIPPED (canProcess = false)");
                        else
                            Debug.Log($"  [{i}] {filter.GetType().Name} → {filter.Process(interactor, interactable)}");
                    }
                }
                else
                {
                    Debug.Log("Interactable Select Filters: <none>");
                }
            }
            else
            {
                Debug.Log("Interactable Select Filters: <not XRBaseInteractable>");
            }

            bool finalResult = manager.CanSelect(interactor, interactable);
            Debug.Log($"FINAL CanSelect RESULT → {finalResult}");
            Debug.Log("===================================");
        }
    }
}
