using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Guerro.Utilities;
using Packet.Guerro.Shared.Characters;
using Stormium.Internal;
using Stormium.Internal.ECS;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

/// <summary>
/// DEBUG CLASS.
/// Class to remove, first, it don't respect the naming, secondly, it's really slow and ugly.
/// </summary>
public class showrigidbodyspeed : MonoBehaviour
{
    private Vector3 m_LastPosition;
    private float m_Speed;

    private int m_LastTickCount;
    private int[] m_Fps = new int[5] {0, 0, 0, 0, 0};
    private int fpsTick = 0;

    private static PlayerLoopSystem system;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OOO()
    {
        ScriptBehaviourUpdateOrder.OnSetPlayerLoop += (pl) =>
        {
            system = pl;

            var indent = 0;
            var text = "";
            LogLoop(system, ref indent, ref text);
            Debug.Log(text);
        };
    }
    
    static void LogLoop(PlayerLoopSystem playerLoopSystem, ref int indent, ref string textToWrite)
    {
        var space = "";
        for (int i = 0; i < indent; i++)
            space += "   ";

        textToWrite += (space + (playerLoopSystem.type == null ? "unknow" : playerLoopSystem.type.Name) + "\n");
            
        if (playerLoopSystem.subSystemList != null && playerLoopSystem.subSystemList.Length > 0)
        {
            indent++;
            textToWrite += (space + "<\n");
            foreach (var sub in playerLoopSystem.subSystemList)
            {
                LogLoop(sub, ref indent, ref textToWrite);
            }
            textToWrite += (space + ">\n");
            indent--;
        }
    }

    private void LateUpdate()
    {

        var flatPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        
        m_Speed = (flatPosition - m_LastPosition).magnitude / Time.deltaTime;
        m_LastPosition = flatPosition;


        if (fpsTick > 4)
            fpsTick = 0;

        m_Fps[fpsTick] = Mathf.FloorToInt(1 / Mathf.Clamp(Time.deltaTime, 0.01f, 2f));
        
        fpsTick++;

        if (Input.GetKeyDown(KeyCode.M))
        {
            var i = 0;
            var text = "";
            LogLoop(system, ref i, ref text);
            Debug.Log(text);
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(10.5f, 10.5f, 200, 25), $"Speed: {m_Speed:F2}");
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 200, 25), $"Speed: {m_Speed:F2}");

        //var rb = GetComponent<GameObjectEntity>().Entity.GetComponentData<Rigidbody>();
        var rb = GetComponent<Rigidbody>();
        
        GUI.color = Color.black;
        GUI.Label(new Rect(10.5f, 21.5f, 200, 25), $"Vel1: {rb.velocity.ToString("F2")}");
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 21, 200, 25), $"Vel1: {rb.velocity.ToString("F2")}");
        
        var character = GetComponent<GameObjectEntity>().Entity.GetComponentData<DCharacterData>();
        
        GUI.color = Color.black;
        GUI.Label(new Rect(10.5f, 32.5f, 200, 25), $"Vel2: {character.RunVelocity.ToString("F2")}");
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 32, 200, 25), $"Vel2: {character.RunVelocity.ToString("F2")}");
        
        var fps = m_Fps.Max();
        
        GUI.color = Color.black;
        GUI.Label(new Rect(10.5f, 50.5f, 200, 25), $"FPS: {fps}");
        GUI.color = fps >= 58 ? Color.green : (fps >= 28 ? Color.yellow : Color.red);
        GUI.Label(new Rect(10, 50, 200, 25), $"FPS: {fps}");
    }
}
