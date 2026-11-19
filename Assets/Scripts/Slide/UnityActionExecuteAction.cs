using UnityEngine;
using UnityEngine.Events;

public sealed class UnityActionExecuteAction : SlideAction
{
	[SerializeField] private UnityEvent _initializeAction = new UnityEvent();
	[SerializeField] private UnityEvent _enterAction = new UnityEvent();
	[SerializeField] private UnityEvent _exitAction = new UnityEvent();
	[SerializeField] private UnityEvent _executeAction = new UnityEvent();
	[SerializeField] private UnityEvent _undoAction = new UnityEvent();
	
	public override void Initialize()
	{
		_initializeAction.Invoke();
	}

	public override void Enter()
	{
		_enterAction.Invoke();
	}

	public override void Exit()
	{
		_exitAction.Invoke();
	}

	public override void Execute()
	{
		_executeAction.Invoke();
	}

	public override void Undo()
	{
		_undoAction.Invoke();
	}
}