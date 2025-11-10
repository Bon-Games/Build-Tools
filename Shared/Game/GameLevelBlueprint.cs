using UnityEngine;
using System.Collections;
using BonGames.Shared;

namespace BonGames
{
    public interface ILevelLoader
    {
        public IEnumerator Load(int level);
        public IEnumerator Unload();
    }

    public interface ICriteria { }

    public class ResourcesLevelLoader : ILevelLoader
    {
        private GameObject _instance;

        public IEnumerator Load(int level)
        {
            yield return Unload();
            ResourceRequest request = Resources.LoadAsync<GameObject>("Levels/Level" + level);
            yield return request;
            _instance = GameObject.Instantiate(request.asset) as GameObject;
            _instance.name = "Level";
            yield break;
        }

        public IEnumerator Unload()
        {
            if (_instance != null)
            {
                GameObject.Destroy(_instance);
                _instance = null;
            }
            yield break;
        }
    }

    public class GameLevel
    {
        private readonly int _level;
        private readonly ILevelLoader _levelLoader;

        public GameLevel(int level, ILevelLoader levelLoader)
        {
            _level = level;
            _levelLoader = levelLoader;
        }

        public CorountineProxy Start()
        {
            return __Start().StartCoroutine();
        }

        public CorountineProxy Unload()
        {
            return _levelLoader.Unload().StartCoroutine();
        }

        private IEnumerator __Start()
        {
            yield return _levelLoader.Load(_level);

            yield break;
        }
    }
}