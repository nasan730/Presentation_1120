public interface ICardVisual
{
	void BindStatus(ICardStatus status);
	void BindNotices(ICardNotices notices);
	void SetIndex(int index);
	void SetDrawInTheForeground(bool isEnabled);
	void SetCanRotation(bool isEnabled);
	void SetReceiveNotice(bool isEnabled);
	void SetShadow(bool isEnabled);
	void SetTintRotate(bool isEnabled);
	void SetHoverRotate(bool isEnabled);
	void SetTextureVisual(bool isEnabled);
	void DestroyProcess();
}
