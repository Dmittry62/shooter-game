using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameUI : MonoBehaviour
{
	public Image fadeImage;
	public GameObject gameOverUI;

	void Start ()
	{
		LivingEntity player = GameObject.FindGameObjectWithTag ("Player").GetComponent<LivingEntity> ();
		player.OnDeath += OnGameOver;
	}

	void OnGameOver ()
	{
		gameOverUI.SetActive (true);
		StartCoroutine (Fade (Color.clear, Color.black, 1f));
	}

	IEnumerator Fade (Color colorFrom, Color colorTo, float time)
	{
		float speed = 1f / time;

		float percent = 0f;

		while (percent < 1f)
		{
			percent += speed * Time.deltaTime;

			fadeImage.color = Color.Lerp (colorFrom, colorTo, percent);
			yield return null;
		}
	}

	public void StartNewGame ()
	{
		SceneManager.LoadScene (0);
	}
}
