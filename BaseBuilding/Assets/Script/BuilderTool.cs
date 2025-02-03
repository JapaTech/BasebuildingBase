using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuilderTool : MonoBehaviour
{
    [SerializeField] private int defaultLayer = 8;
    [SerializeField] private LayerMask buildingLayermask;
    [SerializeField] private LayerMask deleteBuildingLayermask;
    
    [SerializeField] private float rayDistance;
    [SerializeField] private Transform rayOrigin;

    private bool InDeleteMode = false;

    private Camera cam;

    [SerializeField] private Building buildingToSpawn;
    private Building targetBuilding;

    [SerializeField] private Material negativeMaterial;
    [SerializeField] private Material positiveMaterial;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            InDeleteMode = !InDeleteMode;
        }
    }

    private void FixedUpdate()
    {
        if (InDeleteMode)
        {
            DeleteMode();
        }
        else
        {
            BuildMode();
        }
    }

    private bool IsRayHittingSomething(LayerMask layerHitting, out RaycastHit hitInfo)
    {
        var ray = new Ray(rayOrigin.position, cam.transform.forward * rayDistance);

        return Physics.Raycast(ray, out hitInfo, rayDistance, layerHitting);
    }

    private void BuildMode()
    {
        if(targetBuilding != null && targetBuilding.IsFlaggedForDelete)
        {
            targetBuilding.NotReadyForDelete();
        }

        if (buildingToSpawn == null)
            return;

        if (!IsRayHittingSomething(buildingLayermask, out RaycastHit hitInfo))
        {
            buildingToSpawn.ChangeMaterial(negativeMaterial);
        }
        else
        {
            buildingToSpawn.ChangeMaterial(positiveMaterial);
            var positionToSpawn = GridPosition.GridPositionFronWorldPoint(hitInfo.point, 1);
            buildingToSpawn.transform.position = positionToSpawn;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Building building = Instantiate(buildingToSpawn, positionToSpawn, Quaternion.identity);
                building.ResetMaterial();
            }
        }

    }

    private void DeleteMode()
    {
        if (IsRayHittingSomething(deleteBuildingLayermask, out RaycastHit hitInfo))
        {
            var detectedBuilding = hitInfo.collider.gameObject.GetComponentInParent<Building>();

            if (detectedBuilding == null)
                return;

            if(targetBuilding == null)
            {
                targetBuilding = detectedBuilding;
            }

            if(detectedBuilding != targetBuilding && targetBuilding.IsFlaggedForDelete)
            {
                targetBuilding.NotReadyForDelete();
                targetBuilding = detectedBuilding;
            }

            if (detectedBuilding == targetBuilding && !targetBuilding.IsFlaggedForDelete)
            {
                targetBuilding.ReadyForDelete(negativeMaterial);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Destroy(hitInfo.collider.gameObject);
                targetBuilding = null;
            }
        }
        else
        {
            if(targetBuilding != null && targetBuilding.IsFlaggedForDelete)
            {
                targetBuilding.NotReadyForDelete();
                targetBuilding = null;
            }
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + (Vector3.forward * rayDistance)) ;
    }
}
