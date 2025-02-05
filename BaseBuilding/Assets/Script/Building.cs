using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(BoxCollider))]
public class Building : MonoBehaviour
{
    private Renderer r;
    private Material defaultMaterial;
    public bool IsFlaggedForDelete { get; private set; }
    private Building_SO buildingData;

    private BoxCollider boxCollider;
    private GameObject buildingPrefab;

    private Transform colliders;
    public bool IsOverlapping { get; private set; }

    public void Init(Building_SO data)
    {
        buildingData = data;

        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = buildingData.Size;
        boxCollider.center = new Vector3(0, (buildingData.Size.y + 0.2f) * 0.5f, 0);
        boxCollider.isTrigger = true;

        var rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        buildingPrefab = Instantiate(buildingData.Prefab, transform);
        r = buildingPrefab.GetComponentInChildren<Renderer>();
        defaultMaterial = r.material;

        colliders = buildingPrefab.transform.Find("Colliders");

        if (colliders != null)
        {
            colliders.gameObject.SetActive(false);
        }
    }

    public void PlaceBuilding()
    {
        boxCollider.enabled = false;
        if (colliders != null)
        {
            colliders.gameObject.SetActive(true);
        }
        ChangeMaterial(defaultMaterial);
        gameObject.layer = 8;
        gameObject.name = buildingData.BuildingName;
        Debug.Log("Colocou de novo");
    }

    public void ChangeMaterial(Material newMaterial)
    {
        if(r !=null && r.material != newMaterial)
        {
            r.material = newMaterial;
        }
    }

    public void ReadyForDelete(Material negativeMaterial)
    {
        IsFlaggedForDelete = true;
        ChangeMaterial(negativeMaterial);
    }

    public void NotReadyForDelete()
    {
        IsFlaggedForDelete = false;
        ChangeMaterial(defaultMaterial);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 7)
        {
            IsOverlapping = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IsOverlapping = false;
    }
}
