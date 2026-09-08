
using System.Collections.Generic;
using UnityEngine;

public class PalletManager : MonoBehaviour
{

    public Vector3 GridToWorldPosition(int[] pos)
    {
        float x = pos[0] * gridCellSize + 0.5f * gridCellSize;
        float z = - pos[1] * gridCellSize - 0.5f * gridCellSize;
        return new Vector3(x, palletHeight, z);
    }

    [Header("Pallet")]
    [SerializeField] private GameObject palletPrefab;

    [Header("Position")]
    [SerializeField] private float gridCellSize = 1f;
    [SerializeField] private float palletHeight = 1.526f;
    [SerializeField] private float movementSpeed = 3f;
    private Dictionary<int, Vector3> targetPositions =
    new Dictionary<int, Vector3>();

    [Header("Pallet to AGV Mapping")]
    private Dictionary<int, int> palletToAGV =
    new Dictionary<int, int>();
    [SerializeField]
    private AGVManager agvManager;

    // Guarda todos los pallets que actualmente existen en Unity.
    // ID del pallet -> GameObject correspondiente
    private Dictionary<int, GameObject> activePallets =
        new Dictionary<int, GameObject>();


    private void Update()
    {
        foreach (var pair in activePallets)
        {
            int palletId = pair.Key;
            GameObject pallet = pair.Value;

            if (!targetPositions.ContainsKey(palletId))
                continue;

            Vector3 target = targetPositions[palletId];

            pallet.transform.position = Vector3.MoveTowards(
                pallet.transform.position,
                target,
                movementSpeed * Time.deltaTime
            );

            if (palletToAGV.ContainsKey(palletId))
            {
                int agvId = palletToAGV[palletId];

                Transform agv =
                    agvManager.GetAGV(agvId);

                if (agv != null)
                {
                    pallet.transform.rotation =
                        agv.rotation;
                }
            }
        }
    }
    // =========================================================
    // CREAR PALLET
    // =========================================================

    public void CreatePallet(int palletId, Vector3 position)
    {
        // Si ya existe, no lo volvemos a crear.
        if (activePallets.ContainsKey(palletId))
        {
            Debug.LogWarning(
                $"El pallet {palletId} ya existe en Unity."
            );

            return;
        }

        // Crear el GameObject a partir del prefab.
        GameObject newPallet = Instantiate(
            palletPrefab,
            position,
            Quaternion.identity
        );

        // Le ponemos un nombre identificable.
        newPallet.name = $"Pallet_{palletId}";

        // Guardamos la relación:
        // ID Python <-> GameObject Unity
        activePallets.Add(palletId, newPallet);
        targetPositions[palletId] = position;

        Debug.Log($"Pallet {palletId} creado.");
    }


    // =========================================================
    // ACTUALIZAR PALLET
    // =========================================================

    public void UpdatePallet(
        int palletId,
        Vector3 position,
        string status
    )
    {
        // Si está entregado, simplemente lo destruimos
        // si existe. No necesitamos crearlo.
        if (status.Equals("Delivered", System.StringComparison.OrdinalIgnoreCase))
        {
            if (activePallets.ContainsKey(palletId))
            {
                DestroyPallet(palletId);
            }

            return;
        }

        // Si no existe, lo creamos.
        if (!activePallets.ContainsKey(palletId))
        {
            CreatePallet(palletId, position);
        }

        // Obtener el GameObject.
        GameObject pallet = activePallets[palletId];

        // Actualizar posición.
        // Actualizar destino.
        targetPositions[palletId] = position;
    }


    // =========================================================
    // DESTRUIR PALLET
    // =========================================================

    public void DestroyPallet(int palletId)
    {
        if (!activePallets.ContainsKey(palletId))
        {
            Debug.LogWarning(
                $"No se puede destruir el pallet {palletId} porque no existe."
            );

            return;
        }

        // Obtener GameObject.
        GameObject pallet = activePallets[palletId];

        // Destruirlo de la escena.
        Destroy(pallet);

        // Eliminarlo de nuestra lista de pallets activos.
        activePallets.Remove(palletId);
        targetPositions.Remove(palletId);

        Debug.Log($"Pallet {palletId} destruido.");
    }


    // =========================================================
    // COMPROBAR SI EXISTE
    // =========================================================

    public bool HasPallet(int palletId)
    {
        return activePallets.ContainsKey(palletId);
    }


    // =========================================================
    // OBTENER PALLET
    // =========================================================

    public GameObject GetPallet(int palletId)
    {
        if (activePallets.ContainsKey(palletId))
        {
            return activePallets[palletId];
        }

        return null;
    }

    // =========================================================
    // ROTAR PALLET
    // =========================================================

    public void UpdatePalletCarrier(
    int agvId,
    int? palletId
    )
    {
        // El AGV tiene un pallet
        if (palletId.HasValue)
        {
            palletToAGV[palletId.Value] = agvId;
        }
        else
        {
            // El AGV ya no lleva pallet.
            // Quitamos cualquier pallet que estuviera asociado a él.

            List<int> palletsToRemove =
                new List<int>();

            foreach (var pair in palletToAGV)
            {
                if (pair.Value == agvId)
                {
                    palletsToRemove.Add(pair.Key);
                }
            }

            foreach (int palletIdToRemove in palletsToRemove)
            {
                palletToAGV.Remove(palletIdToRemove);
            }
        }
    }
}
