using System;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class GameManager : MonoBehaviour
{
    //Тэги
    public const string PlayerTag = "Player";
    public const string UfoTag = "UFO";
    public const string PlayerBulletTag = "Bullet";
    public const string UfoBulletTag = "UfoBullet";
    public const string AsteroidTag = "Asteroid";
    
    [SerializeField] private float UfoRangeK = 0.8f;
    [SerializeField] private int UfoSpawnTimeMin = 40;
    [SerializeField] private int UfoSpawnTimeMax = 60;
    [SerializeField] private int ExtraPlayer = 1500;

    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    [SerializeField] private Player player;
    [SerializeField] private UFO ufoPrefab;
    [SerializeField] private PoolOfObjects bulletPool;
    [SerializeField] private PoolOfObjects asteroidPool;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private Transform livesPanel;
    [SerializeField] private GameObject lifePrefab;

    public bool GameIsGoing { get; private set; }
    public static Vector2 ScreenResInWorld { get; private set; }

    private int wave = 1;
    private int score;
    private int _newExtraLife;
    private float UfoSpawnRange;

    private void Awake()
    {
        _newExtraLife = ExtraPlayer;
        if (_instance == null) _instance = this;
    }
    
    private void OnEnable()
    {
        var screenResInPixel = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
        ScreenResInWorld = Camera.main.ScreenToWorldPoint(screenResInPixel);
        UfoSpawnRange = ScreenResInWorld.y * UfoRangeK;
        
        Asteroid.SpawnDebris += SpawnDebris;
        PoolOfObjects.SpawnAsteroids += WaitAndSpawnWave;
        Bullet.ScoreChanges += ChangeScore;
        
        gameOverText.text = String.Empty;
        Time.timeScale = 0;
    }
    
    private void OnDisable()
    {
        Asteroid.SpawnDebris -= SpawnDebris;
        PoolOfObjects.SpawnAsteroids -= WaitAndSpawnWave;
        Bullet.ScoreChanges -= ChangeScore;
    }

    private void Start()
    {
        // Считаем настройки префабов
        UFO.FireRateMin = PrefabsConfigContainer.Instance.config.ufoFireRateMin;
        UFO.FireRateMax = PrefabsConfigContainer.Instance.config.ufoFireRateMax;
        UFO.BaseSpeed = PrefabsConfigContainer.Instance.config.ufoBaseSpeed;

        Asteroid.SpeedMin = PrefabsConfigContainer.Instance.config.asteroidSpeedMin;
        Asteroid.SpeedMax = PrefabsConfigContainer.Instance.config.asteroidSpeedMax;
        Asteroid.Tumble = PrefabsConfigContainer.Instance.config.asteroidTumble;

        Bullet.BaseSpeed = PrefabsConfigContainer.Instance.config.bulletBaseSpeed;
        
        Bullet.UfoValue = PrefabsConfigContainer.Instance.config.ufoValue;
        Bullet.LargeAsteroidValue = PrefabsConfigContainer.Instance.config.largeAsteroidValue;
        Bullet.MediumAsteroidValue = PrefabsConfigContainer.Instance.config.mediumAsteroidValue;
        Bullet.SmallAsteroidValue = PrefabsConfigContainer.Instance.config.smallAsteroidValue;

        // Хорошо бы пойти дальше и записывать/считывать PrefabsConfigContainer.Instance.config плюс настройки Player'а и GameManager'а в json на диск
        // В Snake есть реализация записи, но без ScriptableObject, то есть значения переменных инстансов возможно записать только в PlayMode
        // Другими словами в Snake так: переменная инстанса -> переменная класса SaveData -> jsonUtility -> файл
        // Возможно, надо так: переменная инстанса -> поле объекта SaveData в ScriptableObject -> jsonUtility -> файл
        // ScriptableObject же можно ковырять из редактора и сохранять в файл в стопе
        
        // Для префабов возможно так: статическая переменная префаба -> поле объекта SaveData в ScriptableObject -> jsonUtility -> файл
    }

    public void StartGame()
    {
        player.gameObject.SetActive(true);
        player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        player.GetComponent<Collider2D>().enabled = true;
        for (int i = 0; i < player.lives; i++)
        {
            Instantiate(lifePrefab, livesPanel.position, Quaternion.identity, livesPanel);
        }
        
        GameIsGoing = true;
        Time.timeScale = 1;
        SpawnWave();
        Invoke(nameof(SpawnUfo),  Random.Range(UfoSpawnTimeMin, UfoSpawnTimeMax));
    }

    public void ClearOldGame()
    {
        CancelInvoke();
        
        player.gameObject.SetActive(false);
        wave = 1;
        score = 0;
        scoreText.text = "Score: " + score;
        gameOverText.text = String.Empty;

        asteroidPool.ReleaseAll();
        bulletPool.ReleaseAll();

        for (int i = 0; i < livesPanel.childCount; i++)
        {
            Destroy(livesPanel.GetChild(i).gameObject);
        }
        
        foreach (var go in GameObject.FindGameObjectsWithTag(UfoTag))
        {
            Destroy(go);
        }
    }

    private void WaitAndSpawnWave()
    {
        Invoke(nameof(SpawnWave),2f);
    }
    private void SpawnWave()
    {
        for (int i = 0; i <= wave; i++)
        {
            var XorY = Random.value > 0.5f;
            var MinOrMax = Random.value > 0.5f;

            var position = XorY
                ? new Vector2(Random.Range(-ScreenResInWorld.x, ScreenResInWorld.x),
                    MinOrMax ? -ScreenResInWorld.y : ScreenResInWorld.y)
                : new Vector2(MinOrMax ? -ScreenResInWorld.x : ScreenResInWorld.x,
                    Random.Range(-ScreenResInWorld.y, ScreenResInWorld.y)
                );
            
            var aster = SpawnAsteroid(position);
            aster.SetSpeed();
            aster.Move(Random.insideUnitCircle);
        }
        
        SoundsManager.Instance.PlaySound(SoundsContainer.GetAudioClip(SoundTypes.SpawnWave), 0.5f);
        wave++;
    }

    private void SpawnDebris(Vector2 position, Vector2 velocity, Asteroids size)
    {
        var asteroid1 = SpawnAsteroid(position, size);
        var speed = asteroid1.SetSpeed();
        asteroid1.Move((Quaternion.Euler(0, 0, 45) * velocity).normalized);
        //так поворачиваем Vector2 на 45 градусов - (Quaternion.Euler(0, 0, 45) * velocity)
        var asteroid2 = SpawnAsteroid(position, size);
        asteroid2.SetSpeed(speed);
        asteroid2.Move((Quaternion.Euler(0, 0, -45) * velocity).normalized);
    }
    
    private Asteroid SpawnAsteroid(Vector2 position, Asteroids size = Asteroids.Large)
    {
        var trans = asteroidPool.pool.Get().transform;
        var aster = trans.GetComponent<Asteroid>();
        aster.type = size;
        trans.position = position;
        trans.localScale = new Vector3(0.15f * (int)size, 0.15f * (int)size,0.15f * (int)size);
        return aster;
    }
    
    private void SpawnUfo()
    {
        var XorY = Random.value > 0.5f;
        var position = XorY
            ? new Vector2(-ScreenResInWorld.x, Random.Range(-UfoSpawnRange, UfoSpawnRange))
            : new Vector2(ScreenResInWorld.x, Random.Range(-UfoSpawnRange, UfoSpawnRange));
        
        var ufo = Instantiate(ufoPrefab, position, Quaternion.identity);
        ufo.pool = bulletPool.pool;
        ufo.SetSpeed();
        ufo.Move(XorY ? Vector2.right : Vector2.left);
        
        Invoke(nameof(SpawnUfo),  Random.Range(UfoSpawnTimeMin, UfoSpawnTimeMax));
    }

    private void ChangeScore(int value)
    {
        score += value;
        scoreText.text = "Score: " + score;

        if (score < _newExtraLife) return;
        
        player.lives++;
        Instantiate(lifePrefab, livesPanel.position, Quaternion.identity, livesPanel);
        SoundsManager.Instance.PlaySound(SoundsContainer.GetAudioClip(SoundTypes.ExtraPlayer), 0.3f);
        
        _newExtraLife += ExtraPlayer;
    }

    public bool CheckForGameOver()
    {
        player.lives--;
        Destroy(livesPanel.GetChild(0).gameObject);

        if (player.lives == 0)
        {
            GameIsGoing = false;
            player.gameObject.SetActive(false);
            gameOverText.text = "Game Over!";
            MenuManager.Instance.Pause();
            return true;
        }

        return false;
    }
}
