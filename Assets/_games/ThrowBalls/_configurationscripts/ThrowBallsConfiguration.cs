﻿namespace EA4S.ThrowBalls {
    public enum ThrowBallsVariation : int {
        letters = 1,
        words = 2,
        lettersinword = 3,
    }

    public class ThrowBallsConfiguration : IGameConfiguration {
        // Game configuration
        public IGameContext Context { get; set; }
        public IQuestionProvider Questions { get; set; }

        #region Game configurations
        public float Difficulty { get; set; }
        public ThrowBallsVariation Variation { get; set; }
        #endregion

        /////////////////
        // Singleton Pattern
        static ThrowBallsConfiguration instance;
        public static ThrowBallsConfiguration Instance {
            get {
                if (instance == null)
                    instance = new ThrowBallsConfiguration();
                return instance;
            }
        }
        /////////////////

        private ThrowBallsConfiguration() {
            // Default values
            // THESE SETTINGS ARE FOR SAMPLE PURPOSES, THESE VALUES MUST BE SET BY GAME CORE

            Questions = new SampleQuestionProvider();

            Variation = ThrowBallsVariation.letters;

            Context = new SampleGameContext();
            Difficulty = 0.5f;
        }

        #region external configuration call
        public static void SetConfiguration(float _difficulty, int _variation) {
            instance = new ThrowBallsConfiguration() {
                Difficulty = _difficulty,
                Variation = (ThrowBallsVariation)_variation,
            };
        }
        #endregion

        public IQuestionBuilder SetupBuilder() {
            IQuestionBuilder builder = null;
            // TODO
            return builder;
        }
    }
}
