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

    private Camera camera;

    [SerializeField] private GameObject gameObjectToBuild;

    private void Start()
    {
        camera = Camera.main;
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
        var ray = new Ray(rayOrigin.position, camera.transform.forward * rayDistance);

        return Physics.Raycast(ray, out hitInfo, rayDistance, layerHitting);
    }

    private void BuildMode()
    {
        if (gameObjectToBuild == null)
            return;

        if (!IsRayHittingSomething(buildingLayermask, out RaycastHit hitInfo))
            return;

        gameObjectToBuild.transform.position = hitInfo.point;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Instantiate(gameObjectToBuild, hitInfo.point, Quaternion.identity);
        }
    }

    private void DeleteMode()
    {
        if (!IsRayHittingSomething(deleteBuildingLayermask, out RaycastHit hitInfo))
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Destroy(hitInfo.collider.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayOrigin.position, rayDistance * Vector3.forward);
    }
}
