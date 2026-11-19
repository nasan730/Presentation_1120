/**
 *  FileName    :   InspectorName.cs
 *  Description :   プロパティ読み取り専用属性
 *
 *  Copyright 2024 H.Nakata All rights reserved.
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
	/// プロパティ読み取り専用属性
	/// </summary>
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class ReadOnlyAttribute : PropertyAttribute
	{
	}

#if UNITY_EDITOR
	/// <summary>
	/// プロパティ読み取り専用属性: インスペクター描画
	/// </summary>
	[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
	public class ReadOnlyAttributeDrawer : PropertyDrawer
	{
		/// <inheritdoc/>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUI.PropertyField(position, property, label);
			EditorGUI.EndDisabledGroup();
		}
	}
#endif
}