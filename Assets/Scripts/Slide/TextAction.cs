using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public sealed class TextAction : SlideAction
{
	[SerializeField] private TextMeshProUGUI _tmpro;
	[SerializeField] [TextArea] private string _text;

	private string _Text => $"{_text}\n";
	
	public override void Initialize()
	{
		if (_tmpro == null) Assert.IsTrue(false, $"{gameObject.name}：{nameof(TextMeshProUGUI)}の参照がnull");
	}

	public override void Enter()
	{
	}

	public override void Exit()
	{
	}

	public override void Execute()
	{
		_tmpro.text += _Text;
	}

	public override void Undo()
	{
		_tmpro.text = _tmpro.text[..^_Text.Length];
	}
}