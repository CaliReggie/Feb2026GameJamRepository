using UnityEngine;
using UnityEngine.Serialization;


[RequireComponent(typeof(LineRenderer))]
public class ArmLineRenderer : MonoBehaviour
{
    [Header("Inscribed References")]
    
    [SerializeField] private Transform bodyPoint;

    [SerializeField] private Transform armPoint;
    
    [Header("Dynamic References")]
    
    [SerializeField] private LineRenderer lineRenderer;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        
        if (lineRenderer == null ||
            armPoint == null ||
            bodyPoint == null)
        {
            Debug.LogError("ArmLineRenderer is missing references.");
        }
        
        lineRenderer.positionCount = 2;
    }
    
    private void Update()
    {
        lineRenderer.SetPosition(0, bodyPoint.position);
        lineRenderer.SetPosition(1, armPoint.position);
    }
}
