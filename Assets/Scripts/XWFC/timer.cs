using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace XWFC
{
    public class Timer
    {
        private Stopwatch _watch = new Stopwatch();
        

        public void Start(bool verbal = true)
        {
            _watch.Start();
            if (verbal) Debug.Log("Started timer.");
        }

        public float Stop(bool verbal = true)
        {
            _watch.Stop();
            var elapsed = _watch.ElapsedMilliseconds * .001f;
            if (verbal) Debug.Log($"Stopped timer. Elapsed time is {elapsed:0.00} seconds");
            _watch.Reset();
            return elapsed;
        }
    }
}