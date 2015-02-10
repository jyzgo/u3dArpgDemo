/// <summary>
/// Custom InspectorBase base.
/// </summary>
/// 
/// If you want to custom define inspector for your serialized object, you can derive from CustomInspectorBase<DeriveT>
/// Please seen the example below.
/// 
/// [CustomEditor(typeof(LocalizationFontConfig))]
/// public class LocalizationCongfigInspector : CustomInspectorBase<LocalizationFontConfig>
/// {
/// }


using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;


namespace InJoy
{
	public class CustomInspectorBase<DeriveT> : Editor 
		where DeriveT : UnityEngine.Object
	{	
		override public void OnInspectorGUI()
	    {
			DrawCustomEditScript();
			
			DeriveT data = (DeriveT) target;
			
	        GUIContent label = new GUIContent();
	    
			EditorGUI.indentLevel--;
			
	        DrawCustomInspector(label, ref data);
			
			EditorGUI.indentLevel++;
	 
	        if (GUI.changed)
	        {         
	           EditorUtility.SetDirty(target);
	        }
	    }
		
	
	
		private Dictionary<object,bool> _foldoutArray = new Dictionary<object, bool>();
		
		private int _intMinDefault = 0;
		public int IntMinDefault
		{
			set
			{
				_intMinDefault = value;
			}
		}
		
		private int _intMaxDefault = 100;
		public int IntMaxDefault
		{
			set
			{
				_intMaxDefault = value;
			}
		}
		
		protected void DrawCustomEditScript()
		{
			string[] files = System.IO.Directory.GetFiles("Assets", GetType().Name+".cs", System.IO.SearchOption.AllDirectories);

			if (files.Length > 0)
			{
				EditorGUILayout.ObjectField(new GUIContent("Custom Editor Script"), AssetDatabase.LoadAssetAtPath(files[0], typeof(MonoScript)), typeof(MonoScript), false);
			}
		}
		
		private void DrawCustomInspector<T>(GUIContent label, ref T target)
		{		
			Type type = target.GetType();
			FieldInfo[] fields = type.GetFields();
		    
			EditorGUI.indentLevel++;
		 
		    foreach (FieldInfo field in fields)
		    {
				if (field.IsPublic && (Attribute.GetCustomAttribute(field,typeof(System.NonSerializedAttribute)))==null)
		        {
					if (field.FieldType == typeof(int))
		         	{
						int min = _intMinDefault, max = _intMaxDefault;
						object[] attributes = field.GetCustomAttributes(typeof(InJoy.IntMinMaxAttribute), true);
						
						if (attributes.Length > 0)
						{
							min = (attributes[0] as InJoy.IntMinMaxAttribute).Min;
							max = (attributes[0] as InJoy.IntMinMaxAttribute).Max;
						}
						
		            	//field.SetValue(target, EditorGUILayout.IntField(MakeLabel(field), (int)field.GetValue(target)));
						field.SetValue(target, EditorGUILayout.IntSlider(MakeLabel(field), (int)field.GetValue(target), min, max));
		        	} 
					else if(field.FieldType == typeof(float))
		            {
		                field.SetValue(target, EditorGUILayout.FloatField(MakeLabel(field), (float)field.GetValue(target)));
		          	}
					else if (field.FieldType == typeof(string))
					{
						if (field.GetValue(target) == null)
						{
							field.SetValue(target, EditorGUILayout.TextField(MakeLabel(field), ""));
						}
						else
						{
							field.SetValue(target, EditorGUILayout.TextField(MakeLabel(field), (string)field.GetValue(target)));
						}
					}
					else if (field.FieldType == typeof(bool))
					{
						field.SetValue(target, EditorGUILayout.Toggle(MakeLabel(field), (bool)field.GetValue(target)));
					}
					else if (field.FieldType.IsArray)
					{
						Array array = (Array)field.GetValue(target);
						
						DrawArray(field, ref array, field.FieldType.GetElementType());
						
						field.SetValue(target, array);
					}
		         	else if (field.FieldType.IsClass)
		         	{
						DrawClass<T>(field, ref target);
			        }      
			        else
			        {
			            Debug.LogError("InspectorBase<DeriveT> does not support fields of type " + field.FieldType);
			        }
		      	}         
		   }
		 
		   EditorGUI.indentLevel--;
		}
		
		private void DrawClass<T>(FieldInfo field, ref T target)
		{
			EditorGUI.indentLevel++;
			
			Type[] parmTypes = new Type[]{field.FieldType};
			
	    	string methodName = "DrawCustomInspector";
	    	MethodInfo drawMethod = typeof(CustomInspectorBase<DeriveT>).GetMethod(methodName);
	
	        if (drawMethod == null)
	        {
	           Debug.LogError("No method found: " + methodName);
	        }
			
			drawMethod.MakeGenericMethod(parmTypes).Invoke(this, new object[]{MakeLabel(field), field.GetValue(target)});
			
			EditorGUI.indentLevel--;
		}
		
		private void DrawArray(FieldInfo field, ref Array array, System.Type elementType)
		{					
			bool foldout = true;
			if (array != null)
			{
				if (!_foldoutArray.TryGetValue(array, out foldout))
				{
					foldout = true;
				}
			}
			
			if (foldout = EditorGUILayout.Foldout(foldout,MakeLabel(field)))
			{
				EditorGUI.indentLevel++;
			
				int length = 0;
				if (array != null)
				{
					length = array.Length;
				}
				
				length = EditorGUILayout.IntField("Size", length);
				
				ResizeArray(ref array, elementType,length);
				
				if (array != null)
				{
					for (int i=0; i<array.Length; i++)
					{					
						object element = array.GetValue(i);
						
						if (DrawElementLabel(ref element, i))
						{
							DrawCustomInspector(MakeLabel(field), ref element);
						}
					}
				}
				
				EditorGUI.indentLevel--;
			}
			
			if (array != null)
			{
				_foldoutArray[array] = foldout;
			}
		}
		
		private bool DrawElementLabel(ref object element, int index)
		{
			if (element == null)
			{
				return false;
			}
			
			bool foldout = true;
			if (!_foldoutArray.TryGetValue(element, out foldout))
			{
				foldout = true;
			}
			
			Type type = element.GetType();
			FieldInfo[] fields = type.GetFields();
			
			foreach (FieldInfo field in fields)
		    {
				if (field.IsPublic && (Attribute.GetCustomAttribute(field,typeof(System.NonSerializedAttribute)))==null)
		        {
					if (field.FieldType == typeof(string))
					{
						string label = (string)field.GetValue(element);
						
						if (label != string.Empty)
						{
							foldout = EditorGUILayout.Foldout(foldout, label);
						}
						else
						{
							foldout = EditorGUILayout.Foldout(foldout, "Element "+index);
						}
						
						break;
					}
				}
			}
			
			if (element != null)
			{
				_foldoutArray[element] = foldout;
			}
			
			return foldout;
		}
		
		/// <summary>
		/// Resizes the array with new length.
		/// </summary>
		/// <param name='array'>
		/// Array.
		/// </param>
		/// <param name='elementType'>
		/// Element type.
		/// </param>
		/// <param name='length'>
		/// Length.
		/// </param>
		private void ResizeArray(ref Array array, System.Type elementType, int length)
		{
			if ((array==null && length>0) || (array!=null && length!=array.Length))
			{
				Array newArray = Array.CreateInstance(elementType, length);
				
				for (int n=0; n<length; n++)
				{
					if ((array!=null) && (n<array.Length))
					{
						newArray.SetValue(array.GetValue(n), n);
					}
					else 
					{
						newArray.SetValue(Activator.CreateInstance(elementType), n);
					}
				}
				
				array = newArray;
			}
		}
		
		/// <summary>
		/// Makes the label for field.
		/// </summary>
		/// <returns>
		/// The label.
		/// </returns>
		/// <param name='field'>
		/// Field.
		/// </param>
		private GUIContent MakeLabel(FieldInfo field)
		{
		    GUIContent guiContent = new GUIContent();      
		    guiContent.text = field.Name.SplitCamelCase();    
			
			// Trim first char of '_'
			guiContent.text = guiContent.text.TrimStart('_');
			
			// Uppper first char
			string firstChar = guiContent.text[0].ToString();
			guiContent.text = guiContent.text.Remove(0, 1);
			guiContent.text = firstChar.ToUpper() + guiContent.text;
			
		    object[] descriptions = field.GetCustomAttributes(typeof(DescriptionAttribute), true);
		 
		    if (descriptions.Length > 0)
		    {
		        guiContent.tooltip = (descriptions[0] as DescriptionAttribute).Description;
				
				//Debug.Log(guiContent.tooltip);
		    }
		 
		   return guiContent;
		}
	}
	
	static class SplitCamelCaseExtension
	{
		// Split member field name with space character
		public static string SplitCamelCase(this string str)
		{
			return Regex.Replace( Regex.Replace( str, @"(\P{Ll})(\P{Ll}\p{Ll})", "$1 $2" ), @"(\p{Ll})(\P{Ll})", "$1 $2" );
		}
	}
}