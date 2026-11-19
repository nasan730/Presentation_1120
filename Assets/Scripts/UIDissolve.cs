using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIDissolve : MonoBehaviour
{
	private static readonly int DissolveId = Shader.PropertyToID("_Dissolve");

	[SerializeField, Range(0f, 1f)] private float dissolve;

	[SerializeField] private Image _image;
	[SerializeField] private Material _originalMaterial;
	private Material _material;
	private Coroutine _playing;

	public float Dissolve
	{
		get => dissolve;
		set
		{
			dissolve = Mathf.Clamp01(value);
			Apply();
		}
	}

	private void Awake()
	{
		_material = new Material(_originalMaterial);
		_image.material = _material;
		Apply();
	}

	private void OnDestroy()
	{
		if (_material != null) Destroy(_material);
	}

	private void OnValidate()
	{
		if (_material != null) Apply();
	}

	private void Apply()
	{
		if (_material == null) return;
		_material.SetFloat(DissolveId, dissolve);

		// 親に Mask（ステンシル）があると、描画には複製された別マテリアルが使われるため、そちらにも反映する
		var rendering = _image.materialForRendering;
		if (rendering != _material) rendering.SetFloat(DissolveId, dissolve);
	}
	
	public void PlayDissolve(float duration = 0.8f) => Play(1f, duration);

	public void PlayAppear(float duration = 0.8f) => Play(0f, duration);

	private void Play(float to, float duration)
	{
		if (_playing != null) StopCoroutine(_playing);
		_playing = StartCoroutine(PlayRoutine(to, duration));
	}

	private IEnumerator PlayRoutine(float to, float duration)
	{
		var from = dissolve;
		var time = 0f;
		while (time < duration)
		{
			time += Time.deltaTime;
			Dissolve = Mathf.Lerp(from, to, time / duration);
			yield return null;
		}
		Dissolve = to;
		_playing = null;
	}
}