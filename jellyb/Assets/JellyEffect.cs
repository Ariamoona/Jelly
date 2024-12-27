using Unity.VisualScripting;
using UnityEngine;

public class JellyEffect : MonoBehaviour
{
    [Header("Jelly Properties")]
    public float intensity = 1f;
    public float mass = 1f;
    public float stiffness = 1f;
    public float damping = 1f;

    private Mesh originalMesh;
    private Mesh modifiedMesh;
    private MeshRenderer meshRenderer;
    private JellyVertex[] jellyVertices;

    private void Start()
    {
        originalMesh = GetComponent<MeshFilter>().sharedMesh;
        modifiedMesh = Instantiate(originalMesh);
        GetComponent<MeshFilter>().sharedMesh = modifiedMesh;
        meshRenderer = GetComponent<MeshRenderer>();

        InitializeJellyVertices();
    }

    private void InitializeJellyVertices()
    {
        int vertexCount = modifiedMesh.vertices.Length;
        jellyVertices = new JellyVertex[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            Vector3 worldPosition = transform.TransformPoint(modifiedMesh.vertices[i]);
            jellyVertices[i] = new JellyVertex(i, worldPosition);
        }
    }

    private void FixedUpdate()
    {
        Vector3[] originalVertices = originalMesh.vertices;

        for (int i = 0; i < jellyVertices.Length; i++)
        {
            Vector3 targetPosition = transform.TransformPoint(originalVertices[jellyVertices[i].ID]);
            float currentIntensity = CalculateIntensity(targetPosition);

            jellyVertices[i].ApplyForce(targetPosition, mass, stiffness, damping);
            targetPosition = transform.InverseTransformPoint(jellyVertices[i].CurrentPosition);
            originalVertices[jellyVertices[i].ID] = Vector3.Lerp(originalVertices[jellyVertices[i].ID], targetPosition, currentIntensity);
        }

        modifiedMesh.vertices = originalVertices;
    }

    private float CalculateIntensity(Vector3 target)
    {
        return (1 - (meshRenderer.bounds.max.y - target.y) / meshRenderer.bounds.size.y) * intensity;
    }

    public class JellyVertex
    {
        public int ID { get; private set; }
        public Vector3 CurrentPosition { get; private set; }
        private Vector3 velocity;
        private Vector3 force;

        public JellyVertex(int id, Vector3 position)
        {
            ID = id;
            CurrentPosition = position;
        }

        public void ApplyForce(Vector3 target, float mass, float stiffness, float damping)
        {
            force = (target - CurrentPosition) * stiffness;
            velocity = (velocity + force) * damping;
            CurrentPosition += velocity;

            if ((velocity + force / mass).magnitude < 0.001f)
                CurrentPosition = target;
        }
    }
}