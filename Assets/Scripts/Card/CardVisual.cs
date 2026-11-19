using System;
using DG.Tweening;
using N3Library;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour, ICardVisual
{
	[Header("States")]
	[SerializeField] [Min(0.01f)] [Tooltip("追従スピード")] private float _followSpeed = 30.0f;
	[Space]
	[SerializeField] [DisableIsPlaying] private bool _canRotation = true;
	[SerializeField] [Min(0.01f)] [Tooltip("移動量に対する傾きの強さ")] private float rotationAmount = 20;
	[SerializeField] [Min(0.01f)] [Tooltip("傾きが変化する速さ")] private float rotationSpeed = 20; 
	[SerializeField] [Min(0.01f)] [Tooltip("傾きの上限（度）")] private float maxRotation = 60;
	[Space]
	[SerializeField] [DisableIsPlaying] private bool _isDrawInTheForeground = true;
	[Space]
	[SerializeField] [Min(0.0f)] private float _scaleOnHover = 1.15f;
	[SerializeField] [Min(0.0f)] private float _scaleOnSelect = 1.25f;
	[SerializeField] [Min(0.0f)] private float _scaleTransition = 0.15f;
	[SerializeField] private Ease _scaleEase = Ease.OutBack;
	[Space]
	[SerializeField] private float _hoverPunchAngle = 5.0f;
	[SerializeField] private float _hoverTransition = 0.15f;
	[Space]
	[SerializeField] private float _selectPunchAmount = 20.0f;
	[Space]
	[SerializeField] [DisableIsPlaying] private bool _receiveNotice = true;
	[Space]
	[SerializeField] [DisableIsPlaying] private bool _canTintRotate = false;
	[SerializeField] [DisableIsPlaying] private bool _canHoverRotate = false;
	[SerializeField] [Min(0.0f)] private float autoTiltAmount = 30.0f;
	[SerializeField] [Min(0.0f)] private float manualTiltAmount = 20.0f;
	[SerializeField] [Min(0.0f)] private float tiltSpeed = 20.0f;
	
	[Header("Components")]
	[SerializeField] private Transform _shakeParent;
	[SerializeField] private Transform _tiltParent;
	[SerializeField] private Image _imageShadow;
	[SerializeField] private TextMeshProUGUI _tmproIndex;
	[SerializeField] private GameObject _simpleVisual = null;
	[SerializeField] private ShaderCode[] _shaderCodes = Array.Empty<ShaderCode>();
	
	private int _index = 0;
	private bool isHovering = false;
	private ICardStatus _currentStatus = null;
	private Vector3 _movementDelta = Vector3.zero;
	private Vector3 _rotationDelta = Vector3.zero;

	private void Start()
	{
		if (_shakeParent == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(Transform)}の参照がnull");
		if (_tiltParent == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(Transform)}の参照がnull");
		if (_tmproIndex == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(TextMeshProUGUI)}の参照がnull");
		if (_simpleVisual == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(GameObject)}の参照がnull");
		Assert.IsTrue(_shaderCodes.Length > 0, $"{gameObject.name}：{nameof(ShaderCode)}の配列数が0");
		_simpleVisual.gameObject.SetActive(true);
		foreach (var code in _shaderCodes)
		{
			code.Initialize();
			code.gameObject.SetActive(false);
		}
	}
	
	private void Update()
	{
		if (_currentStatus == null) return;
		

		SmoothFollow();
		FollowRotation();
		CardTilt();
		
		if (_isDrawInTheForeground && _currentStatus.IsDragging)
		{
			transform.SetAsLastSibling();
		}
	}

	private void SmoothFollow()
	{
		transform.position = Vector3.Lerp(transform.position, _currentStatus.Transform.position, _followSpeed * Time.deltaTime);
	}
	
	private void FollowRotation()
	{
		if(!_canRotation)
		{
			transform.eulerAngles = Vector3.zero;
			return;
		}
		
		var movement = transform.position - _currentStatus.Transform.position;

		// NOTE: ドラッグ中はマウスの細かい揺れで傾きがガタつくので, 差分をさらに平滑化する
		_movementDelta = Vector3.Lerp(_movementDelta, movement, 25 * Time.deltaTime);
		var movementRotation = (_currentStatus.IsDragging ? _movementDelta : movement) * rotationAmount;

		// 傾き自体もなめらかに変化させる
		_rotationDelta = Vector3.Lerp(_rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);

		// 横方向の遅れ（x）を Z 軸回転に変換. 止まると差が 0 になり, 自然にまっすぐ戻る
		var angleZ = Mathf.Clamp(_rotationDelta.x, -maxRotation, maxRotation);
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angleZ);
	}

	private void CardTilt()
	{
		var idleScale = 0.0f;
		if (_canTintRotate) idleScale = isHovering ? 0.2f : 1.0f;
		
		var sine   = Mathf.Sin(Time.time + _index) * idleScale;
		var cosine = Mathf.Cos(Time.time + _index) * idleScale;

		var tiltX = 0.0f;
		var tiltY = 0.0f;
		if (_canHoverRotate)
		{
			var offset = transform.position - Camera.main.ScreenToWorldPoint(Mouse.current.position.value);
			tiltX = isHovering ? (-offset.y * manualTiltAmount) : 0;
			tiltY = isHovering ? ( offset.x * manualTiltAmount) : 0;
		}

		var lerpX = Mathf.LerpAngle(_tiltParent.eulerAngles.x, tiltX + sine   * autoTiltAmount, tiltSpeed * Time.deltaTime);
		var lerpY = Mathf.LerpAngle(_tiltParent.eulerAngles.y, tiltY + cosine * autoTiltAmount, tiltSpeed * Time.deltaTime);

		_tiltParent.eulerAngles = new Vector3(lerpX, lerpY, _tiltParent.eulerAngles.z);
	}
	
	void ICardVisual.BindStatus(ICardStatus status)
	{
		_currentStatus = status;
		transform.position = _currentStatus.Transform.position;
	}
	
	void ICardVisual.BindNotices(ICardNotices notices)
	{
		notices.PointerEnterEvent.AddListener(PointerEnter);
		notices.PointerExitEvent.AddListener(PointerExit);
		notices.PointerDownEvent.AddListener(PointerDown);
		notices.PointerUpEvent.AddListener(PointerUp);
		notices.BeginDragEvent.AddListener(BeginDrag);
		notices.EndDragEvent.AddListener(EndDrag);
		notices.SelectEvent.AddListener(Select);
	}
	
	void ICardVisual.SetIndex(int index)
	{
		_index = index;
		gameObject.name = gameObject.name.Insert(0, $"[{_index:D2}]");
		_tmproIndex.SetText(_index.ToString("D2"));
	}

	void ICardVisual.SetReceiveNotice(bool isEnabled)
	{
		_receiveNotice = isEnabled;
	}

	void ICardVisual.SetShadow(bool isEnabled)
	{
		_imageShadow.enabled = isEnabled;
	}

	void ICardVisual.SetTintRotate(bool isEnabled)
	{
		_canTintRotate = isEnabled;
	}

	void ICardVisual.SetHoverRotate(bool isEnabled)
	{
		_canHoverRotate = isEnabled;
	}

	void ICardVisual.SetTextureVisual(bool isEnabled)
	{
		_simpleVisual.SetActive(!isEnabled);
		foreach (var code in _shaderCodes)
		{
			code.gameObject.SetActive(isEnabled);
		}
	}

	void ICardVisual.DestroyProcess()
	{
		Destroy(gameObject);
	}
	
	void ICardVisual.SetDrawInTheForeground(bool isEnabled)
	{
		_isDrawInTheForeground = isEnabled;
	}

	void ICardVisual.SetCanRotation(bool isEnabled)
	{
		_canRotation = isEnabled;
	}
	
	private void PointerEnter(ICardStatus card)
	{
		if (!_receiveNotice) return;
		isHovering = true;
		
		ScaleTo(_scaleOnHover);

		_shakeParent.DOKill(true);
		_shakeParent.DOPunchRotation(Vector3.forward * _hoverPunchAngle, _hoverTransition, 20, 1.0f);
	}

	private void PointerExit(ICardStatus card)
	{
		if (!_receiveNotice) return;
		isHovering = false;
		if (!card.WasDragged) ScaleTo(1.0f);
	}

	private void PointerDown(ICardStatus card)
	{
		if (!_receiveNotice) return;
		ScaleTo(_scaleOnSelect);
	}

	private void PointerUp(ICardStatus card, bool longPress)
	{
		if (!_receiveNotice) return;
		ScaleTo(longPress ? _scaleOnHover : _scaleOnSelect);
	}

	private void Select(ICardStatus card, bool state)
	{
		_shakeParent.DOKill(true);

		// 選択時だけ上方向にポンと弾ませる（解除時は 0 なので位置の揺れはなし）
		float dir = state ? 1 : 0;
		_shakeParent.DOPunchPosition(_shakeParent.up * _selectPunchAmount * dir, _scaleTransition, 10, 1.0f);
		_shakeParent.DOPunchRotation(Vector3.forward * (_hoverPunchAngle / 2.0f), _hoverTransition, 20, 1.0f);

		ScaleTo(_scaleOnHover);
	}

	private void BeginDrag(ICardStatus card)
	{
		if (!_receiveNotice) return;
		ScaleTo(_scaleOnSelect);
	}

	private void EndDrag(ICardStatus card)
	{
		if (!_receiveNotice) return;
		ScaleTo(1.0f);
	}

	private void ScaleTo(float scale)
	{
		if (!_receiveNotice) return;
		transform.DOKill();
		transform.DOScale(scale, _scaleTransition).SetEase(_scaleEase);
	}
}