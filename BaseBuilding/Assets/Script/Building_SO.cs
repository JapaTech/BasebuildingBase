using UnityEngine;

[CreateAssetMenu(menuName = "Building System/Building Data", fileName = "Building")]
public class Building_SO : ScriptableObject
{
    public string BuildingName;
    public float GridSize;
    public Vector3 Size;
    public GameObject Prefab;
    public Material defautlMaterial;
}
