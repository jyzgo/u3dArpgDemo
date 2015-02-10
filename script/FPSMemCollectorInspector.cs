using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

[CustomEditor(typeof(FPSMemCollector))]
public class FPSMemCollectorInspector : Editor
{
    public override void OnInspectorGUI()
    {
        _t = target as FPSMemCollector;
        
        DrawTypeControls();
        DrawPeriodControls();
        DrawCacheControls();
        DrawPlatformControls();
        
        if(GUI.changed)
            EditorUtility.SetDirty(_t);
    }
    
    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private FPSMemCollector _t;
    private bool _platformsFoldout;
    
    //////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////

    private void DrawTypeControls()
    {
        _t.CollectorTypeValue = (FPSMemCollector.CollectorType)EditorGUILayout.EnumPopup("Collect type", _t.CollectorTypeValue);        
    }
        
    private void DrawPeriodControls()
    {
        string periodLabel = "";
        switch(_t.CollectorTypeValue)
        {
        case FPSMemCollector.CollectorType.PER_FRAME:
            periodLabel = "frame(s)";
            break;
        case FPSMemCollector.CollectorType.PER_SECOND:
            periodLabel = "second(s)";
            break;
        }
        _t.CollectPeriod = EditorGUILayout.IntField("Each " + periodLabel, _t.CollectPeriod);
    }
    
    private void DrawCacheControls()
    {
        _t.CacheSize = EditorGUILayout.IntField("Cache Size", _t.CacheSize);
    }
    
    private void DrawPlatformControls()
    {
        _platformsFoldout =  EditorGUILayout.Foldout(_platformsFoldout, "Work on platforms");
        if(!_platformsFoldout)
            return;
        
        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            {
                foreach(RuntimePlatform platform in Enum.GetValues(typeof(RuntimePlatform)))
                {
                    bool platformPresentValue = _t.Platforms.Contains(platform);
                    
                    bool newValue = EditorGUILayout.Toggle(
                        Enum.GetName(typeof(RuntimePlatform), platform),
                        platformPresentValue);
                    
                    if(newValue != platformPresentValue)
                    {
                        if(newValue)
                            _t.Platforms.Add(platform);
                        else
                            _t.Platforms.Remove(platform);
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }
}
