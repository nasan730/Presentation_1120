using UnityEngine;
using UnityEngine.InputSystem;

public sealed class SoundController : MonoBehaviour
{
	[SerializeField] private AudioSource _audioSourceYay = null;
	[SerializeField] private AudioSource _audioSourceApplause = null;
	
	private void Update()
	{
		if (Keyboard.current.qKey.wasPressedThisFrame)
		{
			_audioSourceYay.Play();
		}
		if (Keyboard.current.eKey.wasPressedThisFrame)
		{
			_audioSourceApplause.Play();
		}
	}
}