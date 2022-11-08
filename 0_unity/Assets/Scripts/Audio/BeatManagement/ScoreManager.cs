namespace Audio.BeatManagement
{
    public class ScoreManager
    {
        private readonly int _beatsToHitInLevel;
        private int _correctHits;

        public ScoreManager(int beatsToHitInLevel)
        {
            _beatsToHitInLevel = beatsToHitInLevel;
        }
        
        public void AddScore()
        {
            ++_correctHits;
        }
    }
}