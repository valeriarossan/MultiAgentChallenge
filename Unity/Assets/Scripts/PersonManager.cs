using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SimulationManager simulationManager;

    [Header("Prefab y mallas")]
    public GameObject personPrefab; // tiene MeshFilter + MeshRenderer, arranca con idleMesh
    // ANTES:
    // public Mesh idleMesh;
    // public Mesh walkAMesh;
    // public Mesh walkBMesh;

    // AHORA:
    public GameObject idlePosePrefab;
    public GameObject walkAPosePrefab;
    public GameObject walkBPosePrefab;

    private class PersonPoses
    {
        public GameObject root;
        public GameObject idle;
        public GameObject walkA;
        public GameObject walkB;
    }

    private Dictionary<int, PersonPoses> personPoses = new Dictionary<int, PersonPoses>();


    private Dictionary<int, GameObject> personObjects = new Dictionary<int, GameObject>();
    private float yOffset;

    private void Start()
    {
        yOffset = CalculateYOffset();
    }

    private void OnEnable()
    {
        simulationManager.OnFrameUpdated += HandleFrame;
    }

    private void OnDisable()
    {
        simulationManager.OnFrameUpdated -= HandleFrame;
    }


    // Este método reemplaza tu propio Update()/timer:
    // se llama automáticamente cuando SimulationManager avanza de frame.
    private void HandleFrame(FrameData frame)
    {
        AddNewPersons(frame);
        AnimatePersonsToFrame(frame);
    }

    private void AddNewPersons(FrameData frame)
    {
        foreach (PersonData p in frame.persons)
        {
            if (personPoses.ContainsKey(p.id)) continue;

            Vector3 position =
                CellToUnity(
                    p.pos[0],
                    p.pos[1],
                    yOffset
                );

            Quaternion rotation =
                Quaternion.Euler(
                    0,
                    p.orientation + 180f,
                    0
                );

            GameObject root = new GameObject($"Person_{p.id}");
            root.transform.SetPositionAndRotation(position, rotation);

            GameObject idle = Instantiate(idlePosePrefab, root.transform);
            GameObject walkA = Instantiate(walkAPosePrefab, root.transform);
            GameObject walkB = Instantiate(walkBPosePrefab, root.transform);

            idle.transform.localPosition = Vector3.zero;
            walkA.transform.localPosition = Vector3.zero;
            walkB.transform.localPosition = Vector3.zero;

            walkA.SetActive(false);
            walkB.SetActive(false);

            personPoses.Add(p.id, new PersonPoses { root = root, idle = idle, walkA = walkA, walkB = walkB });
        }
    }

    private void AnimatePersonsToFrame(FrameData frame)
    {
        foreach (var entry in personPoses)
        {
            int personId = entry.Key;
            PersonPoses poses = entry.Value;
            PersonData data = frame.persons.Find(p => p.id == personId);

            if (data == null) continue;

            StartCoroutine(AnimatePersonToFrame(poses, data));
        }
    }

    private IEnumerator AnimatePersonToFrame(PersonPoses poses, PersonData nextFramePersonData)
    {
        float frameDuration = simulationManager.SecondsPerFrame;

        if (nextFramePersonData.state == "inactive")
        {
            poses.root.SetActive(false);
            yield break;
        }

        poses.root.SetActive(true);

        Vector3 targetPos =
        CellToUnity(
            nextFramePersonData.pos[0],
            nextFramePersonData.pos[1],
            yOffset
        );

        if (targetPos == poses.root.transform.position)
        {
            SetActivePose(poses, poses.idle);
            yield break;
        }

        Vector3 direction =
            (targetPos - poses.root.transform.position).normalized;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction) *
            Quaternion.Euler(0, 180f, 0);
        bool alreadyFacingTarget = Quaternion.Angle(poses.root.transform.rotation, targetRotation) < 1f;

        if (!alreadyFacingTarget)
        {
            yield return StartCoroutine(RotatePerson(poses.root, targetRotation, frameDuration * 0.2f));
        }

        yield return StartCoroutine(WalkPerson(poses, targetPos, frameDuration * 0.8f));
    }

    private void SetActivePose(PersonPoses poses, GameObject activePose)
    {
        poses.idle.SetActive(activePose == poses.idle);
        poses.walkA.SetActive(activePose == poses.walkA);
        poses.walkB.SetActive(activePose == poses.walkB);
    }

    // ---- Estas dos quedan EXACTAMENTE igual a como las escribió tu amigo ----

    private IEnumerator RotatePerson(GameObject person, Quaternion targetRotation, float time)
    {
        Quaternion startRotation = person.transform.rotation;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            person.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        person.transform.rotation = targetRotation;
    }

    private IEnumerator WalkPerson(PersonPoses poses, Vector3 targetPosition, float time)
    {
        Vector3 startPosition = poses.root.transform.position;
        float elapsed = 0f;
        SetActivePose(poses, poses.walkA);
        bool showingA = true;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            poses.root.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            if (showingA && t >= 0.5f)
            {
                SetActivePose(poses, poses.walkB);
                showingA = false;
            }

            yield return null;
        }

        poses.root.transform.position = targetPosition;
        SetActivePose(poses, poses.idle);
    }

    private float CalculateYOffset()
    {
        MeshFilter mf =
            idlePosePrefab.GetComponentInChildren<MeshFilter>();

        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning(
                "No se encontró MeshFilter/Mesh en idlePosePrefab."
            );

            return 0f;
        }

        return mf.sharedMesh.bounds.size.y / 2f * 0.1f;
    }

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