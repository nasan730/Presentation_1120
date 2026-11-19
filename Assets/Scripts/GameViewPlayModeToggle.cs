#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Play中に W キーで GameView の Play Focused / Play Maximized を切り替える（Editor専用）。
/// 設定値(Enter Play Mode 挙動)の切り替えと、現在の最大化状態の切り替えを同時に行う。
/// </summary>
public static class GameViewPlayModeToggle
{
    // 切り替えキー（必要に応じて変更）
    private const Key ToggleKey = Key.W;

    private static readonly Type GameViewType =
        typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");

    private const BindingFlags Flags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static bool _toggledByScript;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        _toggledByScript = false;

        var go = new GameObject("[GameViewPlayModeToggle]")
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<Runner>();

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private sealed class Runner : MonoBehaviour
    {
        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb[ToggleKey].wasPressedThisFrame) return;
            Toggle();
        }
    }

    private static void Toggle()
    {
        var gameView = FindGameView();
        if (gameView == null)
        {
            Debug.LogWarning("[GameViewPlayModeToggle] GameView が見つかりません。");
            return;
        }

        bool toMaximized = !gameView.maximized;

        SetPlayModeBehavior(gameView, toMaximized);

        // 実際の最大化状態は次フレームで変更（Update中のレイアウト変更を避ける）
        EditorApplication.delayCall += () =>
        {
            if (gameView == null) return;
            gameView.maximized = toMaximized;
            gameView.Focus();
            _toggledByScript = true;
            Debug.Log($"[GameViewPlayModeToggle] {(toMaximized ? "Play Maximized" : "Play Focused")}");
        };
    }

    private static EditorWindow FindGameView()
    {
        if (GameViewType == null) return null;

        // フォーカス中のGameViewを優先
        var focused = EditorWindow.focusedWindow;
        if (focused != null && GameViewType.IsInstanceOfType(focused)) return focused;

        var views = Resources.FindObjectsOfTypeAll(GameViewType);
        return views.Length > 0 ? views[0] as EditorWindow : null;
    }

    /// <summary>GameView の「Play Focused / Play Maximized」設定をリフレクションで変更。</summary>
    private static void SetPlayModeBehavior(EditorWindow gameView, bool maximized)
    {
        // Unity 2020.2+ : PlayModeView.enterPlayModeBehavior (enum: PlayFocused, PlayMaximized, PlayUnfocused)
        var behaviorProp = GameViewType.GetProperty("enterPlayModeBehavior", Flags);
        if (behaviorProp != null && behaviorProp.PropertyType.IsEnum)
        {
            var value = Enum.Parse(behaviorProp.PropertyType, maximized ? "PlayMaximized" : "PlayFocused");
            behaviorProp.SetValue(gameView, value);
            return;
        }

        // 旧バージョン : maximizeOnPlay (bool)
        var maxOnPlayProp = GameViewType.GetProperty("maximizeOnPlay", Flags);
        if (maxOnPlayProp != null && maxOnPlayProp.PropertyType == typeof(bool))
        {
            maxOnPlayProp.SetValue(gameView, maximized);
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) return;

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        if (!_toggledByScript) return;
        _toggledByScript = false;

        // Play中に手動で最大化した場合、Editモードに戻ったら元に戻す
        var gameView = FindGameView();
        if (gameView != null && gameView.maximized)
        {
            gameView.maximized = false;
        }
    }
}
#endif