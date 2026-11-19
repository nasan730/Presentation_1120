using N3Library;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class Card01 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[Header("States")]
	[SerializeField] [ReadOnly] private bool _isDragging = false;

	[Header("Components")]
	[SerializeField] private Image _image = null;

	private Vector3 _offset = Vector3.zero;
	
	private void Start()
	{
		if (_image == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(Image)}の参照がnull");
		_image.raycastTarget = true;
	}
	
	private void Update()
	{
		if (!_isDragging) return;

		Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.value) - _offset;
		transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
	}

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
		_offset = mousePosition - (Vector2)transform.position;

		_isDragging = true;
		_image.raycastTarget = false;
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		// NOTE: IDragHandler を実装しないと OnBeginDrag / OnEndDrag が呼ばれない
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		_isDragging = false;
		_image.raycastTarget = true;
	}
}