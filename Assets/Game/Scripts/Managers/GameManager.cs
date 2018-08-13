using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.MMInterface;
using System;
using Cinemachine;

namespace MoreMountains.LDJAM42
{
	public enum GameStates
	{
		StartScreen,
		LevelStart,
		GameInProgress,
		Score,
		Pause,
		UnPause,
		PlayerDeath,
        AllEnemiesAreDead,
        NoSpaceLeft,
        PreGameOverScreen,
        NextLevel,
        PostNextLevel,
        GameOverScreen
	}

	[Serializable]
	public class FirstUse  
	{
		public bool Used;
	}

	public class GameManager : Singleton<GameManager>
	{
		public MMStateMachine<GameStates> GameState;
        public int StartCountdown = 3;
        public string StartCountdownEndedText = "Go!";
		[Header("Framerate")]
		public int TargetFrameRate = 300;

        public int TotalEnemies = 0;
        public int CurrentEnemies = 0;
        public int DeadEnemies = 0;
        public EnemySpawner LinkedEnemySpawner;

		public bool GamePaused = false;

        public int MaxSpaceLeft = 100;
        public int SpaceLeft = 100;
        public int CurrentLevel = 1;
        public int TotalHouses = 0;

        public GameObject[] Levels;


        [ReadOnly]
		public float TimeSinceStart = 0f;
		protected const float _timescale = 1f;

		protected override void Awake()
		{
			Time.timeScale = _timescale;
			Time.fixedDeltaTime = _timescale * 0.02f;

            CurrentLevel = ProgressManager.Instance.CurrentLevel;
            TotalEnemies = 1 + CurrentLevel*2;
            LinkedEnemySpawner.EnemiesToSpawn = TotalEnemies;
            LinkedEnemySpawner.MaxAtATime = 3 + CurrentLevel;

            base.Awake ();
			Initialization ();
		}

        // STATE MACHINE ------------------------------------------------------------------------------------------------------------------------

		protected virtual void Update()
		{
			if (GameState.CurrentState == GameStates.GameInProgress)
			{
				TimeSinceStart += Time.deltaTime;

                if (SpaceLeft <= MaxSpaceLeft / 5)
                {
                    GUIManager.Instance.NoSpaceLeftWarning(true);
                }

                if (SpaceLeft <= 0)
                {
                    GameState.ChangeState(GameStates.NoSpaceLeft);
                }

                if (DeadEnemies >= TotalEnemies)
                {
                    GameState.ChangeState(GameStates.AllEnemiesAreDead);
                }
			}

            if (GameState.CurrentState == GameStates.PlayerDeath)
            {
                GameManager.Instance.GameState.ChangeState(GameStates.PreGameOverScreen);
                StartCoroutine(GameOver());                
            }

            if (GameState.CurrentState == GameStates.NoSpaceLeft)
            {
                GameManager.Instance.GameState.ChangeState(GameStates.PreGameOverScreen);
                GUIManager.Instance.NoSpaceLeft(true);
                StartCoroutine(GameOver());
            }

            if (GameState.CurrentState == GameStates.AllEnemiesAreDead)
            {
                GameState.ChangeState(GameStates.PostNextLevel);
                GUIManager.Instance.Congratulations(true);
                StartCoroutine(NextLevel());
            }
        }

        // STATE MACHINE ------------------------------------------------------------------------------------------------------------------------

        protected virtual void Initialization()
        {
            Application.targetFrameRate = TargetFrameRate;
            GameState = new MMStateMachine<GameStates>(gameObject, true);
            GUIManager.Instance.FaderOn(false, 1f, false);

            for (int i=0; i< Levels.Length; i++)
            {
                Levels[i].SetActive(false);
            }
            Levels[(CurrentLevel % 3)].SetActive(true);

            if (CurrentLevel == 1)
            {
                //GameState.ChangeState(GameStates.GameInProgress); //TODO REMOVE       
                GUIManager.Instance.SetStartScreen(true);
                GUIManager.Instance.NoSpaceLeftWarning(false);
                GUIManager.Instance.Congratulations(false);
            }
            else
            {
                StartGame();
            }            
        }

		public virtual void StartGame()
		{
			GUIManager.Instance.SetPause (false);
			GamePaused = false;
			GUIManager.Instance.FaderOn (false, 1f, false);
			GameState.ChangeState (GameStates.LevelStart);
			StartCoroutine(Countdown ());

            MoreMountains.TopDownEngine.SFXManager.Instance.UrbanPlanner();
            GUIManager.Instance.SetStartScreen (false);
			GUIManager.Instance.SetGameOverScreen (false);
            GUIManager.Instance.LevelText.text = "Level " + CurrentLevel.ToString();
		}

        protected virtual IEnumerator NextLevel()
        {
            yield return new WaitForSeconds(3f);
            CurrentLevel++;
            ProgressManager.Instance.CurrentLevel++;
            GUIManager.Instance.SetGachaMenu(true);
            //Restart();

        }

		public virtual void BackToMenu()
		{
			Time.timeScale = _timescale;
			GUIManager.Instance.SetPause (false);
			GamePaused = false;
			GUIManager.Instance.SetGameOverScreen (false);
			GUIManager.Instance.SetStartScreen (true);
			GUIManager.Instance.FaderOn (false, 1f, false);
		}

		protected virtual IEnumerator Countdown ()
		{
			float CountDownDelay = 0.25f;

			if (StartCountdown == 0)
			{
				GUIManager.Instance.SetCountdownTextStatus (false);
				GameState.ChangeState (GameStates.GameInProgress);
				yield break;
			}

			GUIManager.Instance.SetCountdownTextStatus (false);
			yield return new WaitForSeconds (CountDownDelay);
			GUIManager.Instance.SetCountdownTextStatus (true);

			int countdown = StartCountdown;
			GUIManager.Instance.SetCountdownTextStatus (true);
			while (countdown > 0)
			{
				GUIManager.Instance.SetCountdownText (countdown.ToString ());
				countdown--;
				yield return new WaitForSeconds (CountDownDelay);
			}
			if (countdown == 0)
			{
				GUIManager.Instance.SetCountdownText (StartCountdownEndedText);
				yield return new WaitForSeconds (CountDownDelay);
			}
			GUIManager.Instance.SetCountdownTextStatus (false);

			GameState.ChangeState (GameStates.GameInProgress);
			TimeSinceStart = 0f;
		}

		public virtual void TriggerPause()
		{
			if (GamePaused)
            {
                MMEventManager.TriggerEvent(new MMTimeScaleEvent(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false));
                GUIManager.Instance.SetPause (false); //TODO RESTORE
				GamePaused = false;
			}
			else
            {
                MMEventManager.TriggerEvent(new MMTimeScaleEvent(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true));
                GUIManager.Instance.SetPause (true); //TODO RESTORE
                GamePaused = true;
			}
		}

		public virtual void Score()
		{
			
			MMEventManager.TriggerEvent (new MMCameraShakeEvent (0.1f, 2f, 50f));
			GameState.ChangeState (GameStates.Score);
			StartCoroutine (GameOver ());
		}

		protected virtual IEnumerator GameOver()
		{
            ProgressManager.Instance.CurrentLevel = 1;
            GUIManager.Instance.NoSpaceLeftWarning(false);
			yield return new WaitForSeconds (2.5f);
			GUIManager.Instance.FaderOn (true, 1f, false);
            GUIManager.Instance.NoSpaceLeft(false);
            GUIManager.Instance.Congratulations(false);
            yield return new WaitForSeconds (2f);

            GameManager.Instance.GameState.ChangeState(GameStates.GameOverScreen);
            GUIManager.Instance.SetGameOverScreen (true);
		}

		protected virtual void Reset()
        {
            MMEventManager.TriggerEvent(new MMTimeScaleEvent(MMTimeScaleMethods.Set, 1f, 0f, false, 0f, false));
            GameState.ChangeState (GameStates.LevelStart);
		}

		public virtual void SelfDestruction()
		{
			Destroy (this.gameObject);
		}	

		public virtual void Restart()
        {
            MMEventManager.TriggerEvent(new MMTimeScaleEvent(MMTimeScaleMethods.Set, 1f, 0f, false, 0f, false));
            LoadingSceneManager.LoadScene ("Main");
			SelfDestruction ();
		}		
	}
}
