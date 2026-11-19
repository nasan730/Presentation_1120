using System;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class ActivateAction : SlideAction
{
	[SerializeField] private GameObject[] _activeateObjects = Array.Empty<GameObject>();
	[SerializeField] private GameObject[] _deactiveateObjects = Array.Empty<GameObject>();

	public override void Initialize()
	{
		foreach (var obj in _activeateObjects)
		{
			if (obj == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(GameObject)}の参照がnull");
			obj.SetActive(false);
		}
		foreach (var obj in _deactiveateObjects)
		{
			if (obj == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(GameObject)}の参照がnull");
			obj.SetActive(true);
		}
	}

	public override void Enter()
	{
	}

	public override void Exit()
	{
	}

	public override void Execute()
	{
		foreach (var obj in _activeateObjects)
		{
			obj.SetActive(true);
		}
		foreach (var obj in _deactiveateObjects)
		{
			obj.SetActive(false);
		}
	}

	public override void Undo()
	{
		foreach (var obj in _activeateObjects)
		{
			obj.SetActive(false);
		}
		foreach (var obj in _deactiveateObjects)
		{
			obj.SetActive(true);
		}
	}
}