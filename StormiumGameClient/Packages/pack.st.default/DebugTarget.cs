using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTarget : MonoBehaviour
{
    private static DebugTarget m_Instance;

    public static Vector3 Target
    {
        get => m_Instance.transform.position;
        set => m_Instance.transform.position = value;
    }

    private void Awake()
    {
        m_Instance = this;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}
