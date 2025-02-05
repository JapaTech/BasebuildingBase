using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.VisualScripting;

public class BuilderUI : MonoBehaviour
{
    [SerializeField] private Building_SO largeBoxData;
    [SerializeField] private Building_SO tallBoxData;
    [SerializeField] private Building_SO sphereData;
    [SerializeField] private TMP_Text text;

    public static System.Action<Building_SO> newBuilding;

    private void OnEnable()
    {
        BuilderTool.changeMode += ChangeStateText;
    }

    private void OnDisable()
    {
        BuilderTool.changeMode -= ChangeStateText;
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            newBuilding?.Invoke(largeBoxData);
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            newBuilding?.Invoke(tallBoxData);
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            newBuilding?.Invoke(sphereData);
        }
    }

    private void ChangeStateText(bool deleteMode)
    {
        if (!deleteMode)
        {
            text.text = "Building Mode";
        }
        else
        {
            text.text = "Delete Mode";

        }
    }
}
