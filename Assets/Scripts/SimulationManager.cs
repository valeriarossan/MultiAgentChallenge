using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class SimulationManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PalletManager palletManager;
    [SerializeField] private UnityTCPClient tcpClient;

    [Header("JSON de respaldo")]
    [SerializeField] private TextAsset simulationJson;

    [Header("Reproducción")]
    [SerializeField] private float secondsPerFrame = 0.5f;
    public float SecondsPerFrame => secondsPerFrame;

    [SerializeField] private bool autoPlay = true;

    public List<FrameData> Frames { get; private set; }

    public int CurrentFrameIndex { get; private set; } = 0;

    public event Action<FrameData> OnFrameUpdated;

    private HashSet<int> palletsInLastFrame =
        new HashSet<int>();


    private void OnEnable()
    {
        if (tcpClient != null)
        {
            tcpClient.OnJsonReceived +=
                LoadSimulationFromJson;
        }
    }


    private void OnDisable()
    {
        if (tcpClient != null)
        {
            tcpClient.OnJsonReceived -=
                LoadSimulationFromJson;
        }
    }


    // =========================================================
    // RECIBIR JSON DESDE TCP
    // =========================================================

    private void LoadSimulationFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError(
                "El JSON recibido está vacío."
            );

            return;
        }

        try
        {
            Frames =
                JsonConvert.DeserializeObject<List<FrameData>>(
                    json
                );

            if (Frames == null || Frames.Count == 0)
            {
                Debug.LogError(
                    "El JSON recibido no contiene frames."
                );

                return;
            }

            Debug.Log(
                $"Simulación recibida por TCP: " +
                $"{Frames.Count} frames."
            );

            CurrentFrameIndex = 0;

            if (autoPlay)
            {
                StartCoroutine(
                    PlaySimulation()
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Error al parsear JSON recibido: " +
                e.Message
            );
        }
    }


    // =========================================================
    // REPRODUCCIÓN
    // =========================================================

    private IEnumerator PlaySimulation()
    {
        while (CurrentFrameIndex < Frames.Count)
        {
            ReceiveFrame(
                Frames[CurrentFrameIndex]
            );

            CurrentFrameIndex++;

            yield return new WaitForSeconds(
                secondsPerFrame
            );
        }

        Debug.Log(
            "Simulación terminada."
        );
    }


    // =========================================================
    // PALLETS
    // =========================================================

    private void ApplyPalletsForFrame(
        FrameData frame)
    {
        HashSet<int> palletsInThisFrame =
            new HashSet<int>();

        foreach (PalletData p in frame.pallets)
        {
            palletsInThisFrame.Add(p.id);

            Vector3 worldPos =
                palletManager.GridToWorldPosition(
                    p.pos
                );

            palletManager.UpdatePallet(
                p.id,
                worldPos,
                p.state
            );
        }

        foreach (int previousId in palletsInLastFrame)
        {
            if (!palletsInThisFrame.Contains(previousId)
                && palletManager.HasPallet(previousId))
            {
                palletManager.DestroyPallet(
                    previousId
                );
            }
        }

        palletsInLastFrame =
            palletsInThisFrame;
    }


    // =========================================================
    // ENVIAR FRAME A LOS DEMÁS MANAGERS
    // =========================================================

    public void ReceiveFrame(
        FrameData frame)
    {
        ApplyPalletsForFrame(frame);

        OnFrameUpdated?.Invoke(frame);
    }
}