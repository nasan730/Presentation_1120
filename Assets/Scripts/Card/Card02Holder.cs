using System.Collections.Generic;
using System.Linq;
using N3Library;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public sealed class Card02Holder : MonoBehaviour
{
	[SerializeField] [ReadOnly] private RectTransform _rectTransform;
	[SerializeField] [ReadOnly] private HorizontalLayoutGroup _layoutGroup;
	[SerializeField] [DisableIsPlaying] private bool _canSwap = true;
	
	private List<ICardStatus> _cards = null;

	private void OnEnable()
	{
		if (_cards == null)
		{
			_cards = new List<ICardStatus>();
			var cards = this.GetComponentsInChildren<Card02>(true);
			int index = 0;
			foreach (var card in cards)
			{
				_cards.Add(card);
				_cards[^1].SetIndex(index);
				index++;
			}
		}
		
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
	}
	
	private void Update()
	{
		if (!_canSwap) return;
		
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
	
	//==========================================
	// UnityEvent Only
	//==========================================

	public void OnSetCanSwapFlag(bool isEnable)
	{
		_canSwap = isEnable;
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		_rectTransform ??= GetComponent<RectTransform>();
		_layoutGroup ??= GetComponent<HorizontalLayoutGroup>();
	}
#endif
}