using System.Collections.Generic;
using System.Linq;
using N3Library;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public sealed class Card03Holder : InitializeMonoBehaviour
{
	[SerializeField] [ReadOnly] private RectTransform _rectTransform;
	[SerializeField] [ReadOnly] private HorizontalLayoutGroup _layoutGroup;
	[SerializeField] [DisableIsPlaying] private RectTransform _rectTransformVisualParent;
	
	private List<ICardStatus> _cards = null;

	public override void Initialize()
	{
		if (_cards == null)
		{
			_cards = new List<ICardStatus>();
			var cards = this.GetComponentsInChildren<Card03>(true);
			int index = 0;
			foreach (var card in cards)
			{
				_cards.Add(card);
				if (_rectTransformVisualParent != null) _cards[^1].SetVisualParent(_rectTransformVisualParent);
				_cards[^1].SetIndex(index);
				index++;
			}
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

	public void OnSetCanRotation(bool isEnable)
	{
		foreach (var card in _cards)
		{
			card.Visual.SetCanRotation(isEnable);
		}
	}
	
	public void OnDrawInTheForeground(bool isEnable)
	{
		foreach (var card in _cards)
		{
			card.Visual.SetDrawInTheForeground(isEnable);
		}
	}

	public void OnCanSendNotice(bool isEnable)
	{
		foreach (var card in _cards)
		{
			card.Visual.SetReceiveNotice(isEnable);
		}
	}
}
