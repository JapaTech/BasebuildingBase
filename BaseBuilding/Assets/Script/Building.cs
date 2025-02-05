using UnityEngine;

[RequireComponent(typeof(BoxCollider))] // Ensures the GameObject has a BoxCollider component
public class Building : MonoBehaviour
{
    private Renderer r; // Renderer component for the building's visual representation
    private Material defaultMaterial; // Stores the default material of the building
    public bool IsFlaggedForDelete { get; private set; } // Tracks if the building is marked for deletion
    private Building_SO buildingData; // ScriptableObject containing building data

    private BoxCollider boxCollider; // BoxCollider component for collision detection
    private GameObject buildingPrefab; // The instantiated building prefab

    private Transform colliders; // Reference to the colliders child object (if any)
    public bool IsOverlapping { get; private set; } // Tracks if the building is overlapping with another object

    // Initializes the building with data from the ScriptableObject
    public void Init(Building_SO data)
    {
        buildingData = data; // Store the building data

        // Set up the BoxCollider
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = buildingData.Size; // Set collider size based on building data
        boxCollider.center = new Vector3(0, (buildingData.Size.y + 0.2f) * 0.5f, 0); // Adjust collider center
        boxCollider.isTrigger = true; // Enable trigger mode for overlap detection

        // Add a kinematic Rigidbody to enable collision detection
        var rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // Instantiate the building prefab and store its renderer and default material
        buildingPrefab = Instantiate(buildingData.Prefab, transform);
        r = buildingPrefab.GetComponentInChildren<Renderer>();
        defaultMaterial = r.material;
        
        // Find the "Colliders" child object (if it exists) and disable it during preview
        colliders = buildingPrefab.transform.Find("Colliders");
        if (colliders != null)
        {
            colliders.gameObject.SetActive(false);
        }
    }

    // Places the building, finalizing its position and enabling colliders
    public void PlaceBuilding()
    {
        boxCollider.enabled = false; // Disable the preview collider
        if (colliders != null)
        {
            colliders.gameObject.SetActive(true);  // Enable the building's colliders
        }
        ChangeMaterial(defaultMaterial); // Revert to the default material
        gameObject.layer = 8; // Change the layer to the "Buildings"
        gameObject.name = buildingData.BuildingName; // Rename the GameObject for clarity
        Debug.Log("Building placed successfully.");
    }

    // Changes the building's material (used for preview and deletion feedback)
    public void ChangeMaterial(Material newMaterial)
    {
        // Check if the renderer and material are valid
        if (r !=null && r.material != newMaterial) 
        {
            r.material = newMaterial; // Apply the new material
        }
    }

    // Marks the building for deletion and changes its material to indicate this
    public void ReadyForDelete(Material negativeMaterial)
    {
        IsFlaggedForDelete = true; // Flag the building for deletion
        ChangeMaterial(negativeMaterial); // Apply the negative material (e.g., red for invalid)
    }

    // Unmarks the building for deletion and reverts to the default material
    public void NotReadyForDelete()
    {
        IsFlaggedForDelete = false; // Unflag the building
        ChangeMaterial(defaultMaterial); // Revert to the default material
    }

    // Called when another collider stays inside the building's trigger collider
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 7) // Ignore objects on layer ground
        {
            IsOverlapping = true; // Set the overlapping flag to true
        }
    }

    // Called when another collider exits the building's trigger collider
    private void OnTriggerExit(Collider other)
    {
        IsOverlapping = false;
    }
}
