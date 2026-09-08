using System.Collections.Generic;
using UnityEngine;

public class OutOfServiceManager : MonoBehaviour
{
    [Header("Objetos de Unity")]

    [SerializeField]
    private List<Transform> docks;

    [SerializeField]
    private List<Transform> productionLines;


    [Header("Marcador")]
    [SerializeField]
    private GameObject outOfServiceMarkerPrefab;


    [Header("Separación del objeto")]
    [SerializeField]
    private float markerGap = 0.1f;
    [SerializeField]
    private float rackHeight = 2.3f;


    [SerializeField]
    private SimulationManager simulationManager;


    private Dictionary<int, GameObject> rackMarkers =
        new Dictionary<int, GameObject>();

    private Dictionary<int, GameObject> dockMarkers =
        new Dictionary<int, GameObject>();

    private Dictionary<int, GameObject> productionLineMarkers =
        new Dictionary<int, GameObject>();


    private void OnEnable()
    {
        if (simulationManager != null)
        {
            simulationManager.OnFrameUpdated += ApplyFrame;
        }
    }


    private void OnDisable()
    {
        if (simulationManager != null)
        {
            simulationManager.OnFrameUpdated -= ApplyFrame;
        }
    }


    // =========================================================
    // RECIBIR FRAME
    // =========================================================

    public void ApplyFrame(FrameData frame)
    {
        // RACKS
        foreach (RackData rack in frame.racks)
        {
            UpdateRackMarker(
                rack.id,
                rack.pos,
                rack.state
            );
        }


        // DOCKS
        foreach (DockData dock in frame.docks)
        {
            UpdateMarker(
                dock.id,
                dock.pos,
                dock.state,
                dockMarkers,
                docks
            );
        }


        // PRODUCTION LINES
        foreach (ProductionLineData line in frame.productionLines)
        {
            UpdateMarker(
                line.id,
                line.pos,
                line.state,
                productionLineMarkers,
                productionLines
            );
        }
    }


    // =========================================================
    // ACTUALIZAR MARCADOR
    // =========================================================

    private void UpdateMarker(
    int id,
    int[] pos,
    string state,
    Dictionary<int, GameObject> markers,
    List<Transform> objects)
    {
        bool outOfService =
            state.Equals(
                "out_of_service",
                System.StringComparison.OrdinalIgnoreCase
            );

        // Si ya no está fuera de servicio, ocultar marcador
        if (!outOfService)
        {
            if (markers.ContainsKey(id))
            {
                markers[id].SetActive(false);
            }

            return;
        }

        // Crear marcador si todavía no existe
        if (!markers.ContainsKey(id))
        {
            GameObject marker =
                Instantiate(outOfServiceMarkerPrefab);

            marker.name = $"OutOfServiceMarker_{id}";

            markers.Add(id, marker);
        }

        // El objeto de Unity SOLO se usa para obtener su altura
        if (id < 0 || id >= objects.Count)
        {
            Debug.LogWarning(
                $"No existe objeto de Unity para el ID {id}."
            );

            return;
        }

        Transform objectTransform = objects[id];

        float topY = GetObjectTop(objectTransform);
        
        // La posición X/Z viene EXCLUSIVAMENTE de Python.
        // No importa que el rack visual mida 2x1.
        Vector3 worldPosition =
            CellToUnity(
                pos[0],
                pos[1],
                topY + markerGap
            );

        Debug.Log(
            $"OUT OF SERVICE | ID={id} | " +
            $"Python cell=[{pos[0]}, {pos[1]}] | " +
            $"topY={topY} | " +
            $"Unity position={worldPosition}"
        );

        markers[id].transform.position = worldPosition;
        markers[id].SetActive(true);
    }

    private void UpdateRackMarker(
    int id,
    int[] pos,
    string state)
    {
        bool outOfService =
            state.Equals(
                "out_of_service",
                System.StringComparison.OrdinalIgnoreCase
            );

        // Si ya no está fuera de servicio, ocultar marcador
        if (!outOfService)
        {
            if (rackMarkers.ContainsKey(id))
            {
                rackMarkers[id].SetActive(false);
            }

            return;
        }

        // Crear marcador si todavía no existe
        if (!rackMarkers.ContainsKey(id))
        {
            GameObject marker =
                Instantiate(outOfServiceMarkerPrefab);

            marker.name =
                $"OutOfServiceMarker_{id}";

            rackMarkers.Add(id, marker);
        }

        // X y Z vienen de Python
        // Y viene de la altura manual del rack
        Vector3 worldPosition =
            CellToUnity(
                pos[0],
                pos[1],
                rackHeight + markerGap
            );

        rackMarkers[id].transform.position =
            worldPosition;

        rackMarkers[id].SetActive(true);
    }

    // =========================================================
    // OBTENER PARTE MÁS ALTA DEL OBJETO
    // =========================================================

    private float GetObjectTop(Transform obj)
    {
        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>();

        float maxY =
            obj.position.y;


        foreach (Renderer renderer in renderers)
        {
            if (renderer.bounds.max.y > maxY)
            {
                maxY =
                    renderer.bounds.max.y;
            }
        }


        return maxY;
    }


    // =========================================================
    // CONVERSIÓN PYTHON → UNITY
    // =========================================================

    private Vector3 CellToUnity(
        int x,
        int y,
        float height)
    {
        return new Vector3(
            x + 0.5f,
            height,
            -y - 0.5f
        );
    }
}