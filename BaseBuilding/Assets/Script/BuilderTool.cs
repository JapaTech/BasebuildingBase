using System;
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
    [SerializeField] private int gridSpacing = 1;
    [SerializeField] private float rotationAmount = 90f;
    private Quaternion lastRotation;
   
    [SerializeField] private Material negativeMaterial;
    [SerializeField] private Material positiveMaterial;

    Transform trBuilding;

    private Action<Vector3> placeAction;
    private Action<Building> deleteAction;
    private Action rotationAction;


    private void Start()
    {
        cam = Camera.main;

        trBuilding = buildingToSpawn.transform;
        lastRotation = buildingToSpawn.transform.rotation;
    }

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            InDeleteMode = !InDeleteMode;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && !InDeleteMode)
        {
            placeAction = PlaceBuilding;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && InDeleteMode)
        {
            deleteAction = DeleteBuilding;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame &&!InDeleteMode)
        {
            rotationAction = RotateCounterClockwise;
        }

        if (Keyboard.current.qKey.wasReleasedThisFrame && !InDeleteMode)
        {
            rotationAction = RotateClockwise;
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
            trBuilding.position = positionToSpawn;

            rotationAction?.Invoke();
            rotationAction = null;

            placeAction?.Invoke(positionToSpawn);
            placeAction = null;
        }

    }

    private void DeleteMode()
    {
        //buildingToSpawn = null;

        if (IsRayHittingSomething(deleteBuildingLayermask, out RaycastHit hitInfo))
        {
            var detectedBuilding = hitInfo.collider.gameObject.GetComponentInParent<Building>();

            if (detectedBuilding == null)
                return;

            if (targetBuilding == null)
            {
                targetBuilding = detectedBuilding;
            }

            if (detectedBuilding != targetBuilding && targetBuilding.IsFlaggedForDelete)
            {
                targetBuilding.NotReadyForDelete();
                targetBuilding = detectedBuilding;
            }

            if (detectedBuilding == targetBuilding && !targetBuilding.IsFlaggedForDelete)
            {
                targetBuilding.ReadyForDelete(negativeMaterial);
            }

            deleteAction?.Invoke(detectedBuilding);
            deleteAction = null;
        }
        else
        {
            if (targetBuilding != null && targetBuilding.IsFlaggedForDelete)
            {
                targetBuilding.NotReadyForDelete();
                targetBuilding = null;
            }
        }

    }



    private void PlaceBuilding(Vector3 positionToSpawn)
    {
        Building building = Instantiate(buildingToSpawn, positionToSpawn, lastRotation);
        building.ResetMaterial();
    }

    private void DeleteBuilding(Building buildingToDelete)
    {
        Destroy(buildingToDelete.gameObject);
        targetBuilding = null;
    }

    private void RotateClockwise()
    {
        trBuilding.Rotate(xAngle: 0, rotationAmount, zAngle: 0);
        lastRotation = trBuilding.rotation;
    }

    private void RotateCounterClockwise()
    {
        trBuilding.Rotate(xAngle: 0, -rotationAmount, zAngle: 0);
        lastRotation = trBuilding.rotation;
    }

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + (Vector3.forward * rayDistance)) ;
    }
}
