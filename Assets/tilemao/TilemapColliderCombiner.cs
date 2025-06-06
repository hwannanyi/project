using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class TilemapColliderCombiner : MonoBehaviour
{
    void Start()
    {
        CombineMeshes();
    }

    void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        // 실제로 사용할 MeshFilter만 리스트로 만듦
        List<CombineInstance> combineList = new List<CombineInstance>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].transform == transform) continue;
            if (meshFilters[i].sharedMesh == null) continue; // null mesh 방지

            // 0.5 offset 보정
            Matrix4x4 correction = Matrix4x4.Translate(-new Vector3(0.5f, 0f, 0.5f));
            CombineInstance ci = new CombineInstance();
            ci.mesh = meshFilters[i].sharedMesh;
            ci.transform = meshFilters[i].transform.localToWorldMatrix * correction;
            combineList.Add(ci);

            // 기존 Collider 제거
            Collider col = meshFilters[i].GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = combinedMesh;

        MeshCollider mc = GetComponent<MeshCollider>();
        mc.sharedMesh = combinedMesh;
    }
}
