using Managers;
using NavMeshPlus.Components;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshManager : Singleton<NavMeshManager>
{
    private NavMeshSurface _meshSurface;

    protected override void Awake()
    {
        base.Awake();
        _meshSurface = GetComponent<NavMeshSurface>();
        _meshSurface.hideEditorLogs = true;
    }

    private void Start()
    {
        // Check if the NavMesh data is already available
        if (_meshSurface.navMeshData == null)
        {
            // If not, build the NavMesh
            _meshSurface.BuildNavMeshAsync();
        }
        // Optionally, if the NavMesh data is already present but you want to rebuild it every time
        else
        {
            _meshSurface.UpdateNavMesh(_meshSurface.navMeshData);
        }
    }

    [ContextMenu("Update Nav Mesh")]
    public void UpdateNavMesh()
    {
        if (_meshSurface.navMeshData != null)
        {
            _meshSurface.UpdateNavMesh(_meshSurface.navMeshData);
        }
        else
        {
            _meshSurface.BuildNavMeshAsync();
        }
    }
}
