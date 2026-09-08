using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// Connects to the Python server, reads the ENTIRE JSON payload
// (server sends it all in one sendall() then closes the connection),
// and writes it to disk as a .json file.
public class UnityTCPClient : MonoBehaviour
{
    public string serverIP = "127.0.0.1";
    // public string serverIP = "10.22.236.58";
    public int serverPort = 1101;

    // Where to save the received JSON (persistentDataPath is safe on all platforms)
    public string outputFileName = "simulation_frames.json";

    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private Thread receiveThread;
    private volatile bool isRunning = true;

    // Set by the background thread, read on the main thread in Update()
    private volatile bool jsonReady = false;
    private string receivedJson = null;

    // Fired on the main thread once the JSON has been written to disk.
    // Subscribe to this instead of reading the file in your own Start().
    public event Action<string> OnJsonReceived;

    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(serverIP, serverPort);
            networkStream = tcpClient.GetStream();

            receiveThread = new Thread(ReceiveFullJson) { IsBackground = true };
            receiveThread.Start();

            Debug.Log("Connected to server.");
        }
        catch (Exception e)
        {
            Debug.LogError("Connection failed: " + e.Message);
        }
    }

    private void ReceiveFullJson()
    {
        try
        {
            byte[] buffer = new byte[4096];
            using (MemoryStream ms = new MemoryStream())
            {
                int bytesRead;
                // Read() blocks until data arrives; returns 0 when the server closes the connection.
                // Since the Python server does one sendall() + close() per connection,
                // looping until bytesRead == 0 gives us the complete payload.
                while (isRunning && (bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, bytesRead);
                }

                string json = Encoding.UTF8.GetString(ms.ToArray());
                receivedJson = json;
                jsonReady = true;
            }
        }
        catch (Exception e)
        {
            if (isRunning)
            {
                Debug.LogError("Receive error: " + e.Message);
            }
        }
    }

    void Update()
    {
        if (jsonReady)
        {
            jsonReady = false; // consume once
            SaveJsonToFile(receivedJson);
        }
    }

    private void SaveJsonToFile(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("Received empty JSON payload, not saving.");
            return;
        }

        try
        {
            string path =
                Path.Combine(
                    Application.persistentDataPath,
                    outputFileName
                );

            File.WriteAllText(path, json);

            Debug.Log(
                "Saved JSON to: " + path
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Failed to save JSON file: " +
                e.Message
            );

            return;
        }

        // Mandar el JSON al SimulationManager
        OnJsonReceived?.Invoke(json);
    }

    void OnApplicationQuit()
    {
        Shutdown();
    }

    void OnDestroy()
    {
        Shutdown();
    }

    private void Shutdown()
    {
        isRunning = false;
        try { networkStream?.Close(); } catch { }
        try { tcpClient?.Close(); } catch { }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join(200); // wait briefly instead of Abort()
        }
    }
}