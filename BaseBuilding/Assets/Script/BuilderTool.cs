using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuilderTool : MonoBehaviour
{
    [SerializeField] private int previewLayer = 9;
    [SerializeField] private LayerMask buildingLayermask;
    [SerializeField] private LayerMask deleteBuildingLayermask;
    
    [SerializeField] private float rayDistance;
    [SerializeField] private Transform rayOrigin;

    private bool inDeleteMode = false;

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

    public static Action<bool> changeMode;

    [SerializeField] private Building_SO testAsset;
    private Building_SO lastAssetData;

    private void OnEnable()
    {
        BuilderUI.newBuilding += CreatePreview;
    }

    private void OnDisable()
    {
        BuilderUI.newBuilding -= CreatePreview;
    }

    private void Start()
    {
        cam = Camera.main;

        CreatePreview(testAsset);
        if(buildingToSpawn != null)
        {
            trBuilding = buildingToSpawn.transform;
        }
        lastRotation = Quaternion.identity;
        changeMode?.Invoke(inDeleteMode);
    }

    private void Update()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            inDeleteMode = !inDeleteMode;
            changeMode?.Invoke(inDeleteMode);
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && !inDeleteMode)
        {
            placeAction = PlaceBuilding;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && inDeleteMode)
        {
            deleteAction = DeleteBuilding;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame &&!inDeleteMode)
        {
            rotationAction = RotateCounterClockwise;
        }

        if (Keyboard.current.qKey.wasReleasedThisFrame && !inDeleteMode)
        {
            rotationAction = RotateClockwise;
        }
    }

    private void FixedUpdate()
    {
        if (inDeleteMode)
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

    private void CreatePreview(Building_SO buildingData)
    {
        if (inDeleteMode) 
        {
            if(targetBuilding != null && targetBuilding.IsFlaggedForDelete)
            {
                targetBuilding.NotReadyForDelete();
            }
            targetBuilding = null;
            inDeleteMode = false;
        }

        if(buildingToSpawn != null)
        {
            Destroy(buildingToSpawn.gameObject);
            buildingToSpawn = null;
        }

        var newBuilding = new GameObject
        {
            layer = previewLayer,
            name = buildingData.name + "Preview",
        };

        buildingToSpawn = newBuilding.AddComponent<Building>();
        buildingToSpawn.Init(buildingData);
        trBuilding = buildingToSpawn.transform;
        trBuilding.rotation = lastRotation;

        lastAssetData = buildingData;
    }

    private void BuildMode()
    {
        if(targetBuilding != null && targetBuilding.IsFlaggedForDelete)
        {
            targetBuilding.NotReadyForDelete();
            targetBuilding = null;
        }

        buildingToSpawn.ChangeMaterial(buildingToSpawn.IsOverlapping ? negativeMaterial : positiveMaterial);        

        if (IsRayHittingSomething(buildingLayermask, out RaycastHit hitInfo))
        {
            var positionToSpawn = GridPosition.GridPositionFronWorldPoint(hitInfo.point, gridSpacing);
            trBuilding.position = positionToSpawn;

            rotationAction?.Invoke();
            rotationAction = null;

            if (!buildingToSpawn.IsOverlapping)
            {
                placeAction?.Invoke(positionToSpawn);
                placeAction = null;
            }
        }
    }

    private void DeleteMode()
    {
        if(buildingToSpawn != null)
        {
            Destroy(buildingToSpawn.gameObject);
            buildingToSpawn = null;
        }


        if(IsRayHittingSomething(deleteBuildingLayermask, out RaycastHit hitInfo))
        {
            var detectedObje = hitInfo.collider.gameObject;
            //Debug.Log(detectedObje);

            var detectedBuilding = hitInfo.collider.gameObject.GetComponentInParent<Building>();
            Debug.Log(detectedBuilding);

            if(detectedBuilding == null)
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

            if (targetBuilding == detectedBuilding && !targetBuilding.IsFlaggedForDelete) 
            {
                targetBuilding.ReadyForDelete(negativeMaterial);
            }

            deleteAction?.Invoke(targetBuilding);
            deleteAction = null;
        }
        else
        {
            if (targetBuilding != null)
            {
                targetBuilding.NotReadyForDelete();
                targetBuilding = null;
            }
        }

    }

    private void PlaceBuilding(Vector3 positionToSpawn)
    {
        buildingToSpawn.PlaceBuilding();
        trBuilding.position = positionToSpawn;
        lastRotation = trBuilding.rotation;
        buildingToSpawn = null;
        if(lastAssetData != null)
        {
            CreatePreview(lastAssetData);
        }
        else
        {
            Debug.Log("O último asset não carregou");
        }
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
        if (cam == null)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + (cam.transform.forward * rayDistance)) ;
    }
}
