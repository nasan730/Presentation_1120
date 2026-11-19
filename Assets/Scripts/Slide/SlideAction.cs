using UnityEngine;

public abstract class SlideAction : MonoBehaviour
{
	public abstract void Initialize();
	public abstract void Enter();
	public abstract void Exit();
	public abstract void Execute();
	public abstract void Undo();
}