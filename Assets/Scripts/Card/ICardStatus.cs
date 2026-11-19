using UnityEngine;

public interface ICardStatus
{
	bool IsDragging { get; }
	bool WasDragged { get; }
	Transform Transform { get; }
	ICardVisual Visual { get; }
	void SetIndex(int index);
	void SetVisualParent(Transform parent);
}