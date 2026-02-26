using UnityEngine;
using UnityEngine.InputSystem;

public class InputTest : MonoBehaviour
{
    public InputActionProperty testActionValue;
    public InputActionProperty testButtonValue;

    void Start()
    {
        
    }


    void Update()
    {
        float value = testActionValue.action.ReadValue<float>();
        Debug.Log(value);

        bool button = testButtonValue.action.IsPressed();
        Debug.Log(button);

    }
}
