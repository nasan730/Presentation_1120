using System;
using UnityEngine.Events;

public interface ICardNotices
{
	UnityEvent<ICardStatus> PointerEnterEvent { get; }
	UnityEvent<ICardStatus> PointerExitEvent { get; }
	UnityEvent<ICardStatus> PointerDownEvent { get; }
	UnityEvent<ICardStatus, bool> PointerUpEvent { get; }
	UnityEvent<ICardStatus> BeginDragEvent { get; }
	UnityEvent<ICardStatus> EndDragEvent { get; }
	UnityEvent<ICardStatus, bool> SelectEvent { get; }
}