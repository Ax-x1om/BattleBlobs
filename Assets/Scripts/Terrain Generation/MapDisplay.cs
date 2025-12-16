using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Mesh;

public class MapDisplay : MonoBehaviour
{
    //public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshCollider = meshFilter.AddComponent<MeshCollider>();
    }
}

