using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using System;

namespace MoreMountains.LDJAM42
{	
	public class GUIManager : MonoBehaviour
	{
		public bool ListenToEvents = true;

		[Header("Essentials")]
		public CanvasGroup Fader;

		[Header("Start")]
		public Text CountdownText;

        [Header("GUI")]
        public MMProgressBar HealthBar;
        public MMProgressBar SpaceLeftBar;
        public GameObject NoSpaceLeftPanel;
        public GameObject NoSpaceLeftWarningPanel;
        public GameObject CongratulationsPanel;
        public Text EnemiesText;
        public Text LevelText;
        public GameObject GachaMenu;

        [Header("Start Screen")]
		public CanvasGroup StartScreen;
		public GameObject OptionsScreen;
		public GameObject HowToPlayScreen;

		[Header("Game Over Screen")]
		public CanvasGroup GameOverScreen;
		public Text GameOverStats;
		public Text GameOverScore;

		[Header("Pause Menu")]
		public GameObject PauseButton;
		public CanvasGroup PauseScreen;
		public Text PauseScore;
		public Text PauseScoreHS;
		public Text PauseLevel;
		public Text PauseLevelHS;

        protected float _lastSpaceBarValue;
        protected float _lastHealthBarValue;

	    protected static GUIManager _instance;

		protected WaitForSeconds TimerUpdateDelay;

		protected int _totalCheckpoints;

		public static GUIManager Instance
		{
			get
			{
				if(_instance == null)
					_instance = GameObject.FindObjectOfType<GUIManager>();
				return _instance;
			}
		}

		protected virtual void Start()
		{
			Initialization ();
		}

		public virtual void Initialization()
		{
			SetPause (false);
			TimerUpdateDelay = new WaitForSeconds (1f);
	    }

        public virtual void UpdateHealthBar(float currentHealth, float maximumHealth)
        {
            if (currentHealth != _lastHealthBarValue)
            {
                HealthBar.UpdateBar(currentHealth, 0f, maximumHealth);
                HealthBar.Bump();
            }
            _lastHealthBarValue = currentHealth;
        }

       /* protected virtual IEnumerator UpdateTimer()
		{
			while (true)
			{
				if (GameManager.Instance.GameState.CurrentState == GameStates.GameInProgress)
				{
					var timeSinceStart = GameManager.Instance.TimeSinceStart;
					var seconds = Mathf.Floor(timeSinceStart % 60);
					var minutes = Mathf.Floor(timeSinceStart / 60);
				}

				yield return TimerUpdateDelay;	
			}
		}*/

		protected virtual void Update()
		{
            if (GameManager.Instance.GameState.CurrentState == GameStates.GameInProgress)
            {
                float spaceLeft = GameManager.Instance.SpaceLeft;
                if (spaceLeft != _lastSpaceBarValue)
                {
                    SpaceLeftBar.UpdateBar(GameManager.Instance.SpaceLeft, 0f, GameManager.Instance.MaxSpaceLeft);
                    SpaceLeftBar.Bump();
                    GameManager.Instance.TotalHouses++;
                }                
                _lastSpaceBarValue = GameManager.Instance.SpaceLeft;
            }

            
		}

		public virtual void BumpEnergyBar()
		{

		}

		public virtual void SetCountdownTextStatus(bool status)
		{
			CountdownText.gameObject.SetActive(status);
		}

		public virtual void SetCountdownText(string newText)
		{
			CountdownText.text = newText;
		}


		public virtual void SetPause(bool state)
		{
			if (PauseScreen == null)
			{
				return;
			}
			PauseScreen.gameObject.SetActive (state);
			PauseScreen.alpha = state ? 1 : 0; 
			PauseScreen.interactable = state;
			PauseScreen.blocksRaycasts = state;
		}

		public virtual void SetHUDActive(bool status)
		{
			PauseButton.gameObject.SetActive (status);
            HealthBar.gameObject.SetActive(status);
            SpaceLeftBar.gameObject.SetActive(status);
            LevelText.gameObject.SetActive(status);
            EnemiesText.gameObject.SetActive(status);
        }

		public virtual void SetStartScreen(bool status) 
		{
			OptionsScreen.SetActive (true);
			HowToPlayScreen.SetActive (true);
			StartScreen.gameObject.SetActive (status);
			SetHUDActive (!status);
		}

        public virtual void Congratulations(bool status)
        {
            CongratulationsPanel.SetActive(status);
        }

        public virtual void NoSpaceLeftWarning(bool status)
        {
            NoSpaceLeftWarningPanel.SetActive(status);
        }

        public virtual void NoSpaceLeft(bool status)
        {
            NoSpaceLeftPanel.SetActive(status);
        }

        public virtual void SetGachaMenu(bool state)
        {
            GachaMenu.SetActive(state);
        }

        public virtual void SetGameOverScreen(bool state)
		{
			GameOverScreen.gameObject.SetActive(state);
			var timeSinceStart = GameManager.Instance.TimeSinceStart;
			var seconds = Mathf.Floor(timeSinceStart % 60);
			var minutes = Mathf.Floor(timeSinceStart / 60);
			GameOverStats.text = String.Format("CONGRATULATIONS,\nYOU REACHED LEVEL "+GameManager.Instance.CurrentLevel + "\nAND BUILT "+GameManager.Instance.TotalHouses+" HOUSES");
		}
				
		public virtual void FaderOn(bool state,float duration, bool unscaled = true)
		{
	        if (Fader== null)
			{
				return;
	        }
			Fader.gameObject.SetActive(true);
			if (state)
			{
				Fader.alpha = 0f;
				StartCoroutine(MMFade.FadeCanvasGroup(Fader,duration, 1f, unscaled));
			}				
			else
			{
				Fader.alpha = 1f;
				StartCoroutine(MMFade.FadeCanvasGroup(Fader,duration,0f, unscaled));
				StartCoroutine (TurnFaderOff (duration));
			}				
		}		

		protected virtual IEnumerator TurnFaderOff(float duration)
		{
			yield return new WaitForSeconds (duration + 0.2f);
			Fader.gameObject.SetActive (false);
		}

		protected virtual void OnEnable()
		{
			
		}

		protected virtual void OnDisable()
		{
			
		}
	}
}