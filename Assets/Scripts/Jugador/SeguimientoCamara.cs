using UnityEngine;

public class SeguimientoCamara : MonoBehaviour
{
    [HideInInspector]
    public Transform objetivo; 

    [Header("Ángulo y Distancia")]
    public Vector3 offset = new Vector3(0f, 7f, -5f);
    public float suavizado = 5f; 

    void LateUpdate()
    {
        if (objetivo == null) return;
        Vector3 posicionDeseada = objetivo.position + offset;
        transform.position = Vector3.Lerp(transform.position, posicionDeseada, suavizado * Time.deltaTime);
        transform.rotation = Quaternion.Euler(55f, 0f, 0f);
    }
}