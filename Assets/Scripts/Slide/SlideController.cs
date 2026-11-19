using System;
using N3Library;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class SlideController : MonoBehaviour
{
	private const int Start_Index = 0;
	
	[SerializeField] [ReadOnly] private int _slideIndex = 0;
	[SerializeField] private Slide[] _slides = Array.Empty<Slide>();
	[SerializeField] private InitializeMonoBehaviour[] _initializers = Array.Empty<InitializeMonoBehaviour>();
	
	private void Start()
	{
		Assert.IsNotNull(_slides, "スライドの配列参照がnull");
		Assert.IsTrue(_slides.Length > 0, "スライドが存在しない");
		for (int i = 0; i < _initializers.Length; i++)
		{
			_initializers[i].Initialize();
		}
		for (int i = 0; i < _slides.Length; i++)
		{
			Assert.IsNotNull(_slides[i], $"{i}番目のスライドの参照がnull");
			_slides[i].Initialize(i);
		}
		_slideIndex = Start_Index;
		UpdateActiveSlide();
	}

	private void Update()
	{
		if (Keyboard.current.sKey.isPressed) return; // skip
		
		var keyboard = Keyboard.current;
		if (keyboard.dKey.wasPressedThisFrame)
		{
			var isNext = _slides[_slideIndex].TryExecute();
			if (isNext)
			{
				_slideIndex = Mathf.Min(_slideIndex + 1, _slides.Length - 1);
				UpdateActiveSlide();
			}
		}
		else if (keyboard.aKey.wasPressedThisFrame)
		{
			var isBack = _slides[_slideIndex].TryUndo();
			if (isBack)
			{
				_slideIndex = Mathf.Max(_slideIndex - 1, 0);
				UpdateActiveSlide();
			}
		}
	}

	private void UpdateActiveSlide()
	{
		for (int i = 0; i < _slides.Length; i++)
		{
			_slides[i].SetActive(i == _slideIndex);
		}
	}
}