public sealed class NonCardVisual : ICardVisual
{
	public void BindStatus(ICardStatus status) { }
	public void BindNotices(ICardNotices notices) { }
	public void SetIndex(int index) { }
	public void SetDrawInTheForeground(bool isEnabled) { }
	public void SetCanRotation(bool isEnabled) { }
	public void SetReceiveNotice(bool isEnabled) { }
	public void SetShadow(bool isEnabled) { }
	public void SetTintRotate(bool isEnabled) { }
	public void SetHoverRotate(bool isEnabled) { }
	public void SetTextureVisual(bool isEnabled) { }
	public void DestroyProcess() { }
}