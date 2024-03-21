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

        public void Stop(bool verbal = true)
        {
            _watch.Stop();
            if (verbal) Debug.Log($"Stopped timer. Elapsed time is {(_watch.ElapsedMilliseconds * .001f):0.00} seconds");
        }
    }
}