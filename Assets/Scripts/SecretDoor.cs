using System.Collections;
using UnityEngine;

public class SecretDoor : MonoBehaviour
{
    public float openAngle = 90f;
    public float duration = 2f;
    
    private bool isOpen = false;
    private bool isAnimating = false;

    public void ToggleDoor()
    {
        if (!isAnimating)
        {
            StartCoroutine(RotateDoor());
        }
    }

    private IEnumerator RotateDoor()
    {
        isAnimating = true;
        Quaternion startRotation = transform.localRotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, isOpen ? -openAngle : openAngle, 0);
        float time = 0;

        while (time < duration)
        {
            transform.localRotation = Quaternion.Slerp(startRotation, endRotation, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = endRotation;
        isOpen = !isOpen;
        isAnimating = false;
    }
}
