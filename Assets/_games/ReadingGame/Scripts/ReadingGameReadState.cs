﻿using System;
using UnityEngine;

namespace EA4S.ReadingGame
{
    public class ReadingGameReadState : IGameState
    {
        CountdownTimer gameTime = new CountdownTimer(90.0f);
        ReadingGameGame game;
        IAudioSource timesUpAudioSource;

        bool hurryUpSfx;

        bool completedDragging = false;
        ReadingBar dragging;
        Vector2 draggingOffset;

        public ReadingGameReadState(ReadingGameGame game)
        {
            this.game = game;

            gameTime.onTimesUp += OnTimesUp;
        }

        public void EnterState()
        {
            game.isTimesUp = false;

            if (game.CurrentQuestionNumber >= ReadingGameGame.MAX_QUESTIONS)
            {
                game.EndGame(game.CurrentStars, game.CurrentScore);
                return;
            }

            ++game.CurrentQuestionNumber;

            // Reset game timer
            gameTime.Reset(ReadingGameGame.TIME_TO_ANSWER);
            gameTime.Start();

            game.Context.GetOverlayWidget().SetClockDuration(gameTime.Duration);
            game.Context.GetOverlayWidget().SetClockTime(gameTime.Time);

            hurryUpSfx = false;

            var inputManager = game.Context.GetInputManager();

            inputManager.onPointerDown += OnPointerDown;
            inputManager.onPointerUp += OnPointerUp;
            
            game.blurredText.SetActive(true);
            //game.circleBox.SetActive(false);

            // Pick a question
            var pack = ReadingGameConfiguration.Instance.Questions.GetNextQuestion();
            game.CurrentQuestion = pack;

            if (pack != null)
                game.barSet.SetData(pack.GetQuestion());
            else
                game.EndGame(game.CurrentStars, game.CurrentScore);

            completedDragging = false;
        }


        public void ExitState()
        {
            var inputManager = game.Context.GetInputManager();

            inputManager.onPointerDown -= OnPointerDown;
            inputManager.onPointerUp -= OnPointerUp;

            if (timesUpAudioSource != null)
                timesUpAudioSource.Stop();

            gameTime.Stop();

            game.barSet.Clear();
            game.blurredText.SetActive(false);
        }

        public void Update(float delta)
        {
            game.Context.GetOverlayWidget().SetClockTime(gameTime.Time);

            if (!hurryUpSfx)
            {
                if (gameTime.Time < 4f)
                {
                    hurryUpSfx = true;

                    timesUpAudioSource = game.Context.GetAudioManager().PlaySound(Sfx.DangerClockLong);
                }
            }

            gameTime.Update(delta);

            if (dragging != null)
            {
                var inputManager = game.Context.GetInputManager();
                completedDragging = dragging.SetGlassScreenPosition(inputManager.LastPointerPosition + draggingOffset);
            }
            else
            {
                if (completedDragging)
                {
                    var completedAllBars = game.barSet.SwitchToNextBar();

                    if (completedAllBars)
                    {
                        // go to Buttons State
                        game.AnswerState.ReadTime = gameTime.Time;
                        game.SetCurrentState(game.AnswerState);
                        return;
                    }
                }

                completedDragging = false;
            }
        }

        public void UpdatePhysics(float delta)
        {

        }

        void OnTimesUp()
        {
            // Time's up!
            game.isTimesUp = true;
            game.Context.GetOverlayWidget().OnClockCompleted();

            // show time's up and back
            game.Context.GetPopupWidget().ShowTimeUp(
                () =>
                {
                    game.SetCurrentState(this);
                    game.Context.GetPopupWidget().Hide();
                });
        }

        void OnPointerDown()
        {
            if (dragging)
                return;

            var inputManager = game.Context.GetInputManager();
            dragging = game.barSet.PickGlass(Camera.main, inputManager.LastPointerPosition);

            if (dragging != null)
                draggingOffset = dragging.GetGlassScreenPosition() - inputManager.LastPointerPosition;
        }

        void OnPointerUp()
        {
            dragging = null;
        }
    }
}