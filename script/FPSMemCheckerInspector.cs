using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(FPSMemChecker))]
public class FPSMemCheckerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        FPSMemChecker t = target as FPSMemChecker;
        
        t.UpdateInterval = EditorGUILayout.FloatField("Update Interval", t.UpdateInterval);
        t.AlignHorz = (FPSMemChecker.AlignHorzEnum)EditorGUILayout.EnumPopup("Align Horz", t.AlignHorz);
        t.AlignVert = (FPSMemChecker.AlignVertEnum)EditorGUILayout.EnumPopup("Align Vert", t.AlignVert);
 
        if(GUI.changed)
            EditorUtility.SetDirty(t);
    }
}
