using System.Collections.Generic;
using System.Linq;
using N3Library;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public sealed class Card04Holder : InitializeMonoBehaviour
{
	[SerializeField] [ReadOnly] private RectTransform _rectTransform;
	[SerializeField] [ReadOnly] private HorizontalLayoutGroup _layoutGroup;
	[SerializeField] [DisableIsPlaying] private RectTransform _rectTransformVisualParent;
	[SerializeField] [DisableIsPlaying] private bool _execFirstSharder = false;
	
	private List<ICardStatus> _cards = null;

	public override void Initialize()
	{
		if (_cards == null)
		{
			_cards = new List<ICardStatus>();
			var cards = this.GetComponentsInChildren<Card>(true);
			int index = 0;
			foreach (var card in cards)
			{
				_cards.Add(card);
				if (_rectTransformVisualParent != null) _cards[^1].SetVisualParent(_rectTransformVisualParent);
				_cards[^1].SetIndex(index);
				index++;
			}
			if (_execFirstSharder) OnSetShadow(true);
		}
		
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
	}
	
	private void Update()
	{
		if(_cards == null) return;
		
		var draggedCard = _cards.FirstOrDefault(card => card.IsDragging);
		if (draggedCard == null) return;

		foreach (var card in _cards)
		{
			if (card == draggedCard) continue;

			var crossedRight = draggedCard.Transform.position.x > card.Transform.position.x
								&& SlotIndex(draggedCard) < SlotIndex(card);
			var crossedLeft = draggedCard.Transform.position.x < card.Transform.position.x
							  && SlotIndex(draggedCard) > SlotIndex(card);

			if (crossedRight || crossedLeft)
			{
				Swap(draggedCard, card);
				break;
			}
		}
	}
	
	private int SlotIndex(ICardStatus card)
	{
		return card.Transform.parent.GetSiblingIndex();
	}

	private void Swap(ICardStatus draggedCard, ICardStatus crossedCard)
	{
		var draggedSlot = draggedCard.Transform.parent;
		var crossedSlot = crossedCard.Transform.parent;

		crossedCard.Transform.SetParent(draggedSlot);
		crossedCard.Transform.localPosition = Vector3.zero;
		draggedCard.Transform.SetParent(crossedSlot);
	}
	
#if UNITY_EDITOR
	private void OnValidate()
	{
		_rectTransform ??= GetComponent<RectTransform>();
		_layoutGroup ??= GetComponent<HorizontalLayoutGroup>();
	}
#endif
	
	//==========================================
	// UnityEvent Only
	//==========================================

	public void OnSetShadow(bool isEnabled)
	{
		foreach (var card in _cards)
		{
			card.Visual.SetShadow(isEnabled);
		}
	}
	
	public void OnSetTintRotate(bool isEnable)
	{
		foreach (var card in _cards)
		{
			card.Visual.SetTintRotate(isEnable);
		}
	}
	
	public void OnSetHoverRotate(bool isEnable)
	{
		foreach (var card in _cards)
		{
			card.Visual.SetHoverRotate(isEnable);
		}
	}

	public void OnSetTextureVisual(bool isEnable)
	{
		foreach (var card in _cards)
		{
			card.Visual.SetTextureVisual(isEnable);
		}
	}
}
