using System.Collections;
using N3Library;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class Card03 : MonoBehaviour, ICardStatus, ICardNotices,
							 IBeginDragHandler, IDragHandler, IEndDragHandler,
							 IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[Header("States")]
	[SerializeField] [ReadOnly] private bool _isDragging;
	[SerializeField] [ReadOnly] private bool _isHovering;
	[SerializeField] [ReadOnly] private bool _selected;
	[SerializeField] [ReadOnly] private bool _wasDragged;
	[SerializeField] [Min(0.01f)] [Tooltip("持ち上げる量")] private float _selectionOffset = 50.0f;
	[SerializeField] [Min(0.01f)] [Tooltip("長押し判定")] private float _longPressSecond = 0.2f;

	[Header("Components")]
	[SerializeField]
	private Image _image;

	[Header("Prefab")]
	[SerializeField] [DisableIsPlaying]
	private CardVisual _visualPrefab;

	private Vector3 _offset;
	private float _pointerDownTime;
	private ICardVisual _visual = new NonCardVisual();

	bool ICardStatus.IsDragging => _isDragging;
	bool ICardStatus.WasDragged => _wasDragged;
	Transform ICardStatus.Transform => transform;
	public UnityEvent<ICardStatus> PointerEnterEvent { get; private set; } = new UnityEvent<ICardStatus>();
	public UnityEvent<ICardStatus> PointerExitEvent { get; private set; } = new UnityEvent<ICardStatus>();
	public UnityEvent<ICardStatus> PointerDownEvent { get; private set; } = new UnityEvent<ICardStatus>();
	public UnityEvent<ICardStatus, bool> PointerUpEvent { get; private set; } = new UnityEvent<ICardStatus, bool>();
	public UnityEvent<ICardStatus> BeginDragEvent { get; private set; } = new UnityEvent<ICardStatus>();
	public UnityEvent<ICardStatus> EndDragEvent { get; private set; } = new UnityEvent<ICardStatus>();
	public UnityEvent<ICardStatus, bool> SelectEvent { get; private set; } = new UnityEvent<ICardStatus, bool>();
	public ICardVisual Visual => _visual;

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

	private void OnDestroy()
	{
		if (_visual == null) return;
		_visual.DestroyProcess();
		_visual = null;
	}

	void ICardStatus.SetVisualParent(Transform parent)
	{
		if (_visual is not NonCardVisual) return;

		_visual = Instantiate(_visualPrefab, transform.position, Quaternion.identity, parent);
		_visual.BindStatus(this);
		_visual.BindNotices(this);
		_visual.SetTintRotate(false);
		_visual.SetHoverRotate(false);
	}

	void ICardStatus.SetIndex(int index)
	{
		gameObject.name = gameObject.name.Insert(0, $"[{index:D2}]");
		if (_visual == null) return;
		_visual.SetIndex(index);
	}
	
	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		BeginDragEvent.Invoke(this);

		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
		_offset = mousePosition - (Vector2)transform.position;

		_isDragging = true;
		_wasDragged = true;
		_image.raycastTarget = false;
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		// NOTE: IDragHandler を実装しないと OnBeginDrag / OnEndDrag が呼ばれない
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		EndDragEvent.Invoke(this);

		_isDragging = false;
		_image.raycastTarget = true;

		transform.localPosition = RestPosition();

		StartCoroutine(ResetWasDragged());
	}

	private IEnumerator ResetWasDragged()
	{
		yield return new WaitForEndOfFrame();
		_wasDragged = false;
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		_isHovering = true;
		PointerEnterEvent.Invoke(this);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		_isHovering = false;
		PointerExitEvent.Invoke(this);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left) return;

		_pointerDownTime = Time.time;
		PointerDownEvent.Invoke(this);
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left) return;

		var longPress = (Time.time - _pointerDownTime) > _longPressSecond;
		PointerUpEvent.Invoke(this, longPress);

		// 長押し・ドラッグ後はクリック扱いしない
		if (longPress || _wasDragged) return;

		_selected = !_selected;
		transform.localPosition = RestPosition();
		SelectEvent.Invoke(this, _selected);
	}

	private Vector3 RestPosition()
	{
		return _selected ? new Vector3(0.0f, _selectionOffset, 0.0f) : Vector3.zero;
	}
}