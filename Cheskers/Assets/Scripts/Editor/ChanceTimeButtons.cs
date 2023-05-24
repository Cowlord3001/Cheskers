using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChanceTime_Manager))]
public class ChanceTimeButtons : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ChanceTime_Manager settings = (ChanceTime_Manager)target;
        if (GUILayout.Button("1: SuddenDeath")) {
            settings.ChanceTime1_SuddentDeath();
        }
        if (GUILayout.Button("2: TeleportationAccident")) {
            settings.ChanceTime2_TeleportationAccident();
        }
        if (GUILayout.Button("3: Comformity")) {
            settings.ChanceTime3_Comformity();
        }
        if (GUILayout.Button("4: ForeignExchange")) {
            settings.ChanceTime4_ForeignExchange();
        }
        if (GUILayout.Button("5: ThornsII")) {
            settings.ChanceTime5_ThornsII();
        }
        if (GUILayout.Button("6: Duel")) {
            settings.ChanceTime6_Duel();
        }
        if (GUILayout.Button("7: Chesker Revolution")) {
            settings.ChanceTime7_CheskerRevolution();
        }
        if (GUILayout.Button("8: StructuralFailure")) {
            settings.ChanceTime8_StructuralFailure();
        }
        if (GUILayout.Button("9: Necromancy")) {
            settings.ChanceTime9_Necromancy();
        }
        if (GUILayout.Button("10: Defectors")) {
            settings.ChanceTime10_Defectors();
        }
    }


}
