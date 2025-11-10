using System;
using System.Collections;
using UnityEngine;

namespace BonGames.Shared
{
    public class CorountineRunner : MonoBehaviour
    {
        /*
         Internally blank   
        */
        public bool IsDestroyed { get; private set; }

        private void OnDestroy()
        {
            IsDestroyed = true;
        }
    }
    public class CorountineProxy : IDisposable
    {
        public static readonly CorountineProxy Completed = new CorountineProxy(null, null);

        private Coroutine _corInstance;
        private MonoBehaviour _runner;
        private IEnumerator _enumerator;

        public bool IsRunning { get; private set; }
        
        public CorountineProxy(MonoBehaviour target, IEnumerator enumerator)
        {
            _runner = target;
            _enumerator = enumerator;
        }

        public CorountineProxy StartCoroutine()
        {
            if (IsRunning || _enumerator == null || _runner == null) return this;

            try
            {
                _corInstance = _runner.StartCoroutine(CoroutineProxy());
            }
            catch (System.Exception e)
            {
                Logger.LogE(e);
            }
            return this;
        }

        public CorountineProxy StopCoroutine()
        {
            if (!IsRunning || _enumerator == null || _runner == null) return this;

            IsRunning = false;

            if (_corInstance != null && _runner != null)
            {
                _runner.StopCoroutine(_corInstance);
            }
            return this;
        }

        private IEnumerator CoroutineProxy()
        {
            IsRunning = true;
            yield return _enumerator;
            IsRunning = false;
            yield break;
        }

        public void Dispose()
        {
            StopCoroutine();
            _enumerator = null;
            _corInstance = null;
            _runner = null;
        }
    }
    public static class CoroutineRunnerExtensions
    {
        private static CorountineRunner s_runner;

        public static CorountineProxy StartCoroutine(this IEnumerator coroutine, MonoBehaviour runner = null)
        {
            try
            {
                if (s_runner == null || s_runner.gameObject == null || s_runner.IsDestroyed)
                {
                    s_runner = new GameObject(typeof(CorountineRunner).Name, typeof(CorountineRunner)).GetComponent<CorountineRunner>();
                }
                runner ??= s_runner;

                return new CorountineProxy(runner, coroutine).StartCoroutine();
            }
            catch
            {
                return CorountineProxy.Completed;
            }
        }
    }    
}
