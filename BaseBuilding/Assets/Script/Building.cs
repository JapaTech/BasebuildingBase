using UnityEngine;

public class Building : MonoBehaviour
{
    private Renderer r;
    [SerializeField] private Material defaultMaterial;
    public bool IsFlaggedForDelete { get; private set; }

    private void Start()
    {
        r = GetComponentInChildren<Renderer>();
        
        if(r == null)
        {
            Debug.Log("Sem render");
        }
    }

    public void ChangeMaterial(Material newMaterial)
    {
        if(r !=null && r.material != newMaterial)
        {
            r.material = newMaterial;
        }
    }

    public void ResetMaterial()
    {
        GetComponentInChildren<Renderer>().material = defaultMaterial;
    }

    public void ReadyForDelete(Material negativeMaterial)
    {
        IsFlaggedForDelete = true;
        ChangeMaterial(negativeMaterial);
    }

    public void NotReadyForDelete()
    {
        IsFlaggedForDelete = false;
        ResetMaterial();
    }
}
