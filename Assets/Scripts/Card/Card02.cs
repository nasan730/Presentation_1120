using N3Library;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class Card02 : MonoBehaviour, ICardStatus, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[Header("States")]
	[SerializeField] [ReadOnly] private bool _isDragging;

	[Header("Components")]
	[SerializeField] private Image _image;
	[SerializeField] private TextMeshProUGUI _tmproIndex;
	
	private Vector3 _offset;
	private ICardVisual _visual = new NonCardVisual();
	
	bool ICardStatus.IsDragging => _isDragging;
	public bool WasDragged => false;
	Transform ICardStatus.Transform => transform;
	ICardVisual ICardStatus.Visual => _visual;

	private void Start()
	{
		if (_image == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(Image)}の参照がnull");
		if (_tmproIndex == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(TextMeshProUGUI)}の参照がnull");
		_image.raycastTarget = true;
	}
	
	private void Update()
	{
		if (!_isDragging) return;

		Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.value) - _offset;
		transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
	}
	
	void ICardStatus.SetIndex(int index)
	{
		gameObject.name = gameObject.name.Insert(0, $"[{index:D2}]");
		_tmproIndex.SetText(index.ToString("D2"));
	}

	void ICardStatus.SetVisualParent(Transform parent)
	{
		// NOTE: Card02ではロジックとビジュアルが一体化しているため処理なし
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
		
		transform.localPosition = Vector3.zero;
	}
}