using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(Collider))]
public class GrassPainter : MonoBehaviour
{
    [Header("Mask / Painting")]
    public Texture2D mask; // gán từ Mask Painter Window

    [Header("Spawning")]
    public Mesh grassMesh;
    public Material grassMaterial;
    public float spawnDensity = 0.5f;
    public float instanceScaleMin = 0.8f;
    public float instanceScaleMax = 1.2f;
    public int gridResolution = 256;
    public float yOffset = 0.01f;


}
