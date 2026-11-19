using System;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class MultiAction : SlideAction
{
	[SerializeField] private SlideAction[] _actions = Array.Empty<SlideAction>();
	
	public override void Initialize()
	{
		foreach (var action in _actions)
		{
			if (action == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(action)}の参照がnull");
			action.Initialize();
		}
	}

	public override void Enter()
	{
		foreach (var action in _actions)
		{
			action.Enter();
		}
	}

	public override void Exit()
	{
		foreach (var action in _actions)
		{
			action.Exit();
		}
	}

	public override void Execute()
	{
		foreach (var action in _actions)
		{
			action.Execute();
		}
	}

	public override void Undo()
	{
		foreach (var action in _actions)
		{
			action.Undo();
		}
	}
}