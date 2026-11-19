/**
 *  FileName    :   DisableIsPlaying.cs
 *  Description :   ゲーム実行中読み取り専用属性
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
	/// ゲーム実行中読み取り専用属性
	/// </summary>
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class DisableIsPlayingAttribute : PropertyAttribute
	{
	}

#if UNITY_EDITOR
	/// <summary>
	/// ゲーム実行中読み取り専用属性: インスペクター描画
	/// </summary>
	[CustomPropertyDrawer(typeof(DisableIsPlayingAttribute))]
	public class DisableIsPlayingDrawer : PropertyDrawer
	{
		/// <inheritdoc/>
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// 値更新
			property.serializedObject.Update();

			// 属性取得
			var customAttribute = attribute as DisableIsPlayingAttribute;
			if (customAttribute == null)
			{
				EditorGUI.PropertyField(position, property, label);
				property.serializedObject.ApplyModifiedProperties();
				return;
			}

			EditorGUI.BeginDisabledGroup(Application.isPlaying);
			EditorGUI.PropertyField(position, property, label);
			EditorGUI.EndDisabledGroup();

			property.serializedObject.ApplyModifiedProperties();
		}
	}
#endif
}