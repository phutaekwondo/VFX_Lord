using UnityEngine;

public class MainCamera : MonoBehaviour
{
    Portal[] m_portals;
    Camera m_mainCamera;

    void Awake()
    {
        m_portals = FindObjectsOfType<Portal>();
        m_mainCamera = GetComponent<Camera>();
    }
}
