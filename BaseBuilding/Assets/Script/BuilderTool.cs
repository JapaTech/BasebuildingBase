using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuilderTool : MonoBehaviour
{
    [SerializeField] private int previewLayer = 9; // Layer for preview objects
    [SerializeField] private LayerMask buildingLayermask; // Layer mask for detecting where to build
    [SerializeField] private LayerMask deleteBuildingLayermask; // Layer mask for detecting buildings to delete

    [SerializeField] private float rayDistance; // Maximum distance for raycasting
    [SerializeField] private Transform rayOrigin; // Origin point for raycasting

    private bool inDeleteMode = false;  // Tracks whether the tool is in delete mode

    private Camera cam; // Main camera reference

    [SerializeField] private Building buildingToSpawn; // The building prefab to spawn
    [SerializeField] private int gridSpacing = 1; // Grid spacing for snapping buildings
    [SerializeField] private float rotationAmount = 90f; // Amount to rotate buildings
    private Transform trBuilding; // Transform of the building being previewed
    private Building buildingToDelete; // Reference to the building marked for deletion
    private Quaternion lastRotation; // Stores the last rotation applied to the building

    public static Action<bool> changeMode; // Event to notify mode changes (build/delete)

    [SerializeField] private Material negativeMaterial; // Material for invalid placement or delete
    [SerializeField] private Material positiveMaterial; // Material for valid placement

    [SerializeField] private Building_SO testAsset; // Test building asset data for preview
    private Building_SO lastAssetData; // Stores the last building asset data used

    private void OnEnable()
    {
        // Subscribe to the event for creating a new building preview
        BuilderUI.buildingMode += CreatePreview;
    }

    private void OnDisable()
    {
        // Unsubscribe from the event to avoid memory leaks
        BuilderUI.buildingMode -= CreatePreview;
    }

    private void Start()
    {
        cam = Camera.main; // Get the main camera

        // Create a preview of the test building asset
        CreatePreview(testAsset);

        // Initialize the building transform if a building is set

        if (buildingToSpawn != null) 
        {
            trBuilding = buildingToSpawn.transform;
        }

        // Initialize the last rotation
        lastRotation = Quaternion.identity;

        // Notify listeners about the initial mode
        changeMode?.Invoke(inDeleteMode);
    }

    private void Update()
    {
        // Toggle between build and delete modes based on the current mode
        if (inDeleteMode)
        {
            DeleteMode();
        }
        else
        {
            BuildMode();
        }

        // Toggle delete mode when the 'F' key is pressed
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            inDeleteMode = !inDeleteMode;
            changeMode?.Invoke(inDeleteMode);
        }
    }

    // Checks if the ray hits something on the specified layer
    private bool IsRayHittingSomething(LayerMask layerHitting, out RaycastHit hitInfo)
    {
        var ray = new Ray(rayOrigin.position, cam.transform.forward * rayDistance);
        return Physics.Raycast(ray, out hitInfo, rayDistance, layerHitting);
    }

    // Creates a preview of the building based on the provided building data
    private void CreatePreview(Building_SO buildingData)
    {
        // Exit delete mode, if active
        if (inDeleteMode) 
        {
            if(buildingToDelete != null && buildingToDelete.IsFlaggedForDelete)
            {
                buildingToDelete.NotReadyForDelete();
            }
            buildingToDelete = null;
            inDeleteMode = false;
        }

        // Destroy the existing preview building if it exists
        if (buildingToSpawn != null)
        {
            Destroy(buildingToSpawn.gameObject);
            buildingToSpawn = null;
        }

        // Create a new preview building object
        var newBuilding = new GameObject
        {
            layer = previewLayer,
            name = buildingData.name + "Preview",
        };

        // Add the Building component and initialize it with the building data
        buildingToSpawn = newBuilding.AddComponent<Building>();
        buildingToSpawn.Init(buildingData);
        trBuilding = buildingToSpawn.transform;
        trBuilding.rotation = lastRotation;

        // Store the last building asset data
        lastAssetData = buildingData;
    }

    // Handles the building placement logic
    private void BuildMode()
    {
        // Reset the delete mode state if a building is flagged for deletion
        if (buildingToDelete != null && buildingToDelete.IsFlaggedForDelete)
        {
            buildingToDelete.NotReadyForDelete();
            buildingToDelete = null;
        }

        // Change the building material based on whether it's overlapping
        buildingToSpawn.ChangeMaterial(buildingToSpawn.IsOverlapping ? negativeMaterial : positiveMaterial);

        // Check if the ray hits a valid spot for building
        if (IsRayHittingSomething(buildingLayermask, out RaycastHit hitInfo))
        {
            // Snap the building position to the grid
            var positionToSpawn = GridPosition.GridPositionFronWorldPoint(hitInfo.point, gridSpacing);
            trBuilding.position = positionToSpawn;

            // Rotate the building counterclockwise when 'E' is pressed
            if (Keyboard.current.eKey.wasPressedThisFrame && !inDeleteMode)
            {
                RotateCounterClockwise();
            }

            // Rotate the building clockwise when 'Q' is released
            if (Keyboard.current.qKey.wasReleasedThisFrame && !inDeleteMode)
            {
                RotateClockwise();
            }

            // Place the building if it's not overlapping and the left mouse button is pressed
            if (!buildingToSpawn.IsOverlapping && Mouse.current.leftButton.wasPressedThisFrame && !inDeleteMode)
            {
                PlaceBuilding(positionToSpawn);
            }
        }
    }

    // Handles the building deletion logic
    private void DeleteMode()
    {
        // Destroy the preview building if it exists
        if (buildingToSpawn != null)
        {
            Destroy(buildingToSpawn.gameObject);
            buildingToSpawn = null;
        }

        // Check if the ray hits a building that can be deleted
        if (IsRayHittingSomething(deleteBuildingLayermask, out RaycastHit hitInfo))
        {
            // Get the building component from the hit object
            var detectedBuilding = hitInfo.collider.gameObject.GetComponentInParent<Building>();
            Debug.Log(detectedBuilding);

            if(detectedBuilding == null)
                return;

            // Mark the building for deletion
            if (buildingToDelete == null)
            {
                buildingToDelete = detectedBuilding;
            }

            // Create a preview of the test building asset
            if (detectedBuilding != buildingToDelete && buildingToDelete.IsFlaggedForDelete)
            {
                buildingToDelete.NotReadyForDelete();
                buildingToDelete = detectedBuilding;
            }

            // Highlight the building for deletion
            if (buildingToDelete == detectedBuilding && !buildingToDelete.IsFlaggedForDelete) 
            {
                buildingToDelete.ReadyForDelete(negativeMaterial);
            }

            // Delete the building when the left mouse button is pressed
            if (Mouse.current.leftButton.wasPressedThisFrame && inDeleteMode)
            {
                DeleteBuilding(buildingToDelete);
            }
        }
        // Reset the building marked for deletion if the ray doesn't hit anything
        else if (buildingToDelete != null)
        {
            buildingToDelete.NotReadyForDelete();
            buildingToDelete = null;    
        }

    }

    // Places the building at the specified position

    private void PlaceBuilding(Vector3 positionToSpawn)
    {
        buildingToSpawn.PlaceBuilding();
        trBuilding.position = positionToSpawn;
        lastRotation = trBuilding.rotation;
        buildingToSpawn = null;

        // Create a new preview building after placing the current one
        if (lastAssetData != null)
        {
            CreatePreview(lastAssetData);
        }
        else
        {
            Debug.Log("O último asset não carregou");
        }
    }

    // Deletes the specified building
    private void DeleteBuilding(Building buildingToDelete)
    {
        Destroy(buildingToDelete.gameObject);
        this.buildingToDelete = null;
    }

    // Rotates the building clockwise
    private void RotateClockwise()
    {
        trBuilding.Rotate(xAngle: 0, rotationAmount, zAngle: 0);
        lastRotation = trBuilding.rotation;
    }

    // Rotates the building counterclockwise
    private void RotateCounterClockwise()
    {
        trBuilding.Rotate(xAngle: 0, -rotationAmount, zAngle: 0);
        lastRotation = trBuilding.rotation;
    }

    // Draws a debug ray in the editor
    private void OnDrawGizmos()
    {
        if (cam == null)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + (cam.transform.forward * rayDistance)) ;
    }
}
