using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Image))]
public class ShaderCode : MonoBehaviour
{
	[SerializeField] private Image _image;
	[SerializeField] private Material _originalMaterial;
	private Material _material;

	public void Initialize()
	{
		_material = new Material(_originalMaterial);
		_image.material = _material;

		string[] editions = new string[4];
		editions[0] = "REGULAR";
		editions[1] = "POLYCHROME";
		editions[2] = "REGULAR";
		editions[3] = "NEGATIVE";

		for (int i = 0; i < _image.material.enabledKeywords.Length; i++)
		{
			_image.material.DisableKeyword(_image.material.enabledKeywords[i]);
		}

		_image.material.EnableKeyword("_EDITION_" + editions[Random.Range(1, editions.Length)]);
	}

	private void Update()
	{
		var currentRotation = transform.parent.localRotation;
		var eulerAngles = currentRotation.eulerAngles;

		var xAngle = eulerAngles.x;
		var yAngle = eulerAngles.y;

		xAngle = ClampAngle(xAngle, -90f, 90f);
		yAngle = ClampAngle(yAngle, -90f, 90);

		_material.SetVector("_Rotation", new Vector2(ExtensionMethods.Remap(xAngle, -20, 20, -.5f, .5f), ExtensionMethods.Remap(yAngle, -20, 20, -.5f, .5f)));
	}
	
	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -180f) angle += 360f;
		if (angle > 180f) angle -= 360f;
		return Mathf.Clamp(angle, min, max);
	}
	
	private void OnDestroy()
	{
		if (_material != null)
		{
			Destroy(_material);
			_material = null;
			_image.material = null;
		}
	}
}