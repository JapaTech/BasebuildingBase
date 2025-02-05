using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class BuilderUI : MonoBehaviour
{
    // References to ScriptableObjects for different building types
    [SerializeField] private Building_SO largeBoxData;
    [SerializeField] private Building_SO tallBoxData;
    [SerializeField] private Building_SO sphereData;


    [SerializeField] private TMP_Text text;// UI text to display the current mode (build/delete)

    // Static event to notify when a building mode is selected
    public static System.Action<Building_SO> buildingMode; 

    private void OnEnable()
    {
        // Subscribe to the event that notifies when the build/delete mode changes
        BuilderTool.changeMode += ChangeStateText;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event to avoid memory leaks
        BuilderTool.changeMode -= ChangeStateText;
    }

    private void Update()
    {
        // Check for key presses to switch between building types
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            // Trigger the event for the large box building
            buildingMode?.Invoke(largeBoxData);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            // Trigger the event for the tall box building
            buildingMode?.Invoke(tallBoxData);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            // Trigger the event for the sphere building
            buildingMode?.Invoke(sphereData);
        }
    }

    // Updates the UI text to reflect the current mode (build/delete)
    private void ChangeStateText(bool deleteMode)
    {
        if (!deleteMode)
        {
            text.text = "Building Mode"; // Set text to "Building Mode"
        }
        else
        {
            text.text = "Delete Mode"; // Set text to "Delete Mode"

        }
    }
}
