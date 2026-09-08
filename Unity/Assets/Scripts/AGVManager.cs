using System.Collections.Generic;
using UnityEngine;

public class AGVManager : MonoBehaviour
{
    [Header("AGVs en Unity")]
    [SerializeField]
    private List<Transform> agvs;

    [Header("Movimiento")]
    [SerializeField]
    private float movementSpeed = 3f;

    private Dictionary<int, Vector3> targetPositions =
        new Dictionary<int, Vector3>();

    [SerializeField]
    private SimulationManager simulationManager;
    [SerializeField]
    private PalletManager palletManager;

    private Dictionary<int, Transform> agvObjects =
        new Dictionary<int, Transform>();


    private void Start()
    {
        for (int i = 0; i < agvs.Count; i++)
        {
            agvObjects[i] = agvs[i];

            // Al inicio, el destino es su posición actual
            targetPositions[i] = agvs[i].position;
        }
    }


    private void OnEnable()
    {
        simulationManager.OnFrameUpdated += ApplyFrame;
    }


    private void OnDisable()
    {
        simulationManager.OnFrameUpdated -= ApplyFrame;
    }


    private void Update()
    {
        foreach (var pair in agvObjects)
        {
            int id = pair.Key;
            Transform agv = pair.Value;

            if (!targetPositions.ContainsKey(id))
                continue;

            Vector3 target = targetPositions[id];

            agv.position = Vector3.MoveTowards(
                agv.position,
                target,
                movementSpeed * Time.deltaTime
            );
        }
    }


    public void ApplyFrame(FrameData frame)
    {
        foreach (AgvData agv in frame.avgs)
        {
            ApplyAGV(agv);
        }
    }


    private void ApplyAGV(AgvData data)
    {
        if (!agvObjects.ContainsKey(data.id))
        {
            Debug.LogWarning(
                "No Unity AGV found for ID " +
                data.id
            );

            return;
        }

        Transform agv = agvObjects[data.id];

        Vector3 newTargetPosition =
            CellToUnity(
                data.pos[0],
                data.pos[1],
                agv.position.y
            );

        targetPositions[data.id] = newTargetPosition;

        SetOrientation(agv, data.orientation);

        Debug.Log(
            "AGV " + data.id +
            " -> cell [" +
            data.pos[0] + ", " +
            data.pos[1] + "]"
        );
        palletManager.UpdatePalletCarrier(
            data.id,
            data.pallet_id
        );
    }


    private Vector3 CellToUnity(
        int x,
        int y,
        float height
    )
    {
        return new Vector3(
            x + 0.5f,
            height,
            -y - 0.5f
        );
    }


    private void SetOrientation(
        Transform agv,
        int orientation
    )
    {
        agv.rotation = Quaternion.Euler(0, orientation, 0);
    }
    public Transform GetAGV(int agvId)
    {
        if (agvObjects.ContainsKey(agvId))
        {
            return agvObjects[agvId];
        }

        return null;
    }
}