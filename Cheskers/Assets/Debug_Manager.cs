using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_Manager : MonoBehaviour
{
    public enum type
    {
        PlayerCurrentState,
        TurnStateMessages,
        NetworkEvents
    }

    public static Debug_Manager instance;

    [SerializeField] DebugBoolean[] debugBools;

    public Dictionary<type, bool> debugBool_Dictionary;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        debugBool_Dictionary = new Dictionary<type, bool>();
        foreach (var debugBool in debugBools) {
            debugBool_Dictionary.Add(debugBool.type, debugBool.value);
        }
    }

    public void Log(type debugType, string msg)
    {
        if (debugBool_Dictionary[debugType] == true) {
            Debug.Log(msg);
        }
    }

    //References here should be eventually turned into their own function
    public bool GetDebugPermission(type debugType)
    {
        return debugBool_Dictionary[debugType];
    }

    public void Pause(type debugType)
    {
        if (debugBool_Dictionary[debugType] == true) {
            Debug.Break();
        }
    }
}

[System.Serializable]
struct DebugBoolean
{
    public Debug_Manager.type type;
    public bool value;
}
