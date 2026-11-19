using System;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class EnabledAction : SlideAction
{
	[SerializeField] private MonoBehaviour[] _enabledComponents = Array.Empty<MonoBehaviour>();
	[SerializeField] private MonoBehaviour[] _disabledComponents = Array.Empty<MonoBehaviour>();

	public override void Initialize()
	{
		foreach (var component in _enabledComponents)
		{
			if (component == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(Component)}の参照がnull");
			component.enabled = false;
		}
		foreach (var component in _disabledComponents)
		{
			if (component == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(Component)}の参照がnull");
			component.enabled = true;
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
		foreach (var component in _enabledComponents)
		{
			component.enabled = true;
		}
		foreach (var component in _disabledComponents)
		{
			component.enabled = false;
		}
	}

	public override void Undo()
	{
		foreach (var component in _enabledComponents)
		{
			component.enabled = false;
		}
		foreach (var component in _disabledComponents)
		{
			component.enabled = true;
		}
	}
}