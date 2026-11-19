/**
 *  FileName    :   TEMPLATEMonoBehaviour.cs
 *  Description :   個別アセットウィンドウ表示属性
 *
 *  Copyright 2025 H.Nakata All rights reserved.
 */


using System;
using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace N3Library
{
	/// <summary>
	/// 個別アセットウィンドウ表示属性
	/// </summary>
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class OpenPropertyWindowAttribute : PropertyAttribute
	{
		public string LabelName { get; private set; }
		public bool IsReadOnly { get; private set; }
		public OpenPropertyWindowAttribute(string labelName = "", bool isReadOnly = false)
		{
			LabelName = labelName;
			IsReadOnly = isReadOnly;
		}
	}


#if UNITY_EDITOR
	/// <summary>
	/// 個別アセットウィンドウ表示属性: インスペクター描画
	/// </summary>
	[CustomPropertyDrawer(typeof(OpenPropertyWindowAttribute))]
	public class OpenPropertyWindowAttributeDrawer : PropertyDrawer
	{
		/// <inheritdoc/>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			// 値更新
			property.serializedObject.Update();

			// 属性取得
			var customAttribute = attribute as OpenPropertyWindowAttribute;
			if (customAttribute == null)
			{
			    EditorGUI.PropertyField(position, property, label);
			    property.serializedObject.ApplyModifiedProperties();
			    return;
			}

			// 描画更新
			if (property.boxedValue is UnityEngine.Object obj)
			{
				var rectObjectField = position;
				using (new EditorGUI.DisabledScope(customAttribute.IsReadOnly))
				{
					rectObjectField.width = EditorGUIUtility.currentViewWidth - EditorGUILayoutDefine.Width_Icon_Button;
					if (!string.IsNullOrEmpty(customAttribute.LabelName))
					{
						label.text = customAttribute.LabelName;
					}
					EditorGUI.PropertyField(rectObjectField, property, label);
				}
				var rectButton = position;
				rectButton.x = rectObjectField.x + rectObjectField.width;
				rectButton.width = EditorGUILayoutDefine.Width_Icon_Button;
				if (GUI.Button(rectButton, "開く"))
				{
					EditorUtility.OpenPropertyEditor(obj);
				}
			}
			else
			{
				using (new EditorGUI.DisabledScope(customAttribute.IsReadOnly))
				{
					EditorGUI.PropertyField(position, property, label);
				}
			}

			property.serializedObject.ApplyModifiedProperties();
			EditorGUI.EndProperty();
		}
	}
#endif
}
