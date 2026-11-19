using System;
using N3Library;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Slide : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _slideIndexView;
	[SerializeField] private Image _temporaryBg;
	[SerializeField] [ReadOnly] private int _actionIndex;
	[SerializeField] private SlideAction[] _actions = Array.Empty<SlideAction>();

	public void Initialize(int index)
	{
		if (_temporaryBg != null) _temporaryBg.enabled = false;
		Assert.IsNotNull(_slideIndexView, "TextMeshProの参照がnull");
		_slideIndexView.SetText(index.ToString());

		foreach (var action in _actions)
		{
			action.Initialize();
		}

		_actionIndex = 0;
	}

	public void SetActive(bool active)
	{
		gameObject.SetActive(active);

		foreach (var action in _actions)
		{
			if (active) action.Enter();
			else action.Exit();
		}
	}

	public bool TryExecute()
	{
		if (_actionIndex >= _actions.Length) return true;
		_actions[_actionIndex].Execute();
		_actionIndex++;
		return false;
	}

	public bool TryUndo()
	{
		if (_actionIndex <= 0) return true;
		_actionIndex--;
		_actions[_actionIndex].Undo();
		return false;
	}
}