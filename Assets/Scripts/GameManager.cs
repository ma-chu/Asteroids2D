using System;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class GameManager : MonoBehaviour
{
    private const float UfoRangeK = 0.8f;
    private const int UfoSpawnTimeMin = 40;
    private const int UfoSpawnTimeMax = 60;
    private const int ExtraPlayer = 1500;

    private static GameManager _instance;
    public static GameManager Instance => _instance;
    
    public static Vector2 ScreenResInWorld { get; private set; }
    public static float UfoSpawnRange { get; private set; }
    
    [SerializeField] private Player player;
    [SerializeField] private UFO ufoPrefab;
    [SerializeField] private PoolOfObjects bulletPool;
    [SerializeField] private PoolOfObjects asteroidPool;
    
    [SerializeField] private  TMP_Text scoreText;
    [SerializeField] private  TMP_Text gameOverText;
    [SerializeField] private Transform livesPanel;
    [SerializeField] private GameObject lifePrefab;

    public bool GameIsGoing { get; private set; }
    private int wave = 1;
    private int score;
    private int _newExtraLife = ExtraPlayer;

    private void Awake()
    {
        if (_instance == null) _instance = this;
    }
    
    private void OnEnable()
    {
        var screenResInPixel = new Vector2(Camera.main.pixelWidth, Camera.main.pixelHeight);
        ScreenResInWorld = Camera.main.ScreenToWorldPoint(screenResInPixel);
        UfoSpawnRange = ScreenResInWorld.y * UfoRangeK;
        
        Asteroid.SpawnDebris += SpawnDebris;
        PoolOfObjects.SpawnAsteroids += SpawnWave;
        Bullet.ScoreChanges += ChangeScore;
        
        gameOverText.text = String.Empty;
        Time.timeScale = 0;
    }
    
    private void OnDisable()
    {
        Asteroid.SpawnDebris -= SpawnDebris;
        PoolOfObjects.SpawnAsteroids -= SpawnWave;
        Bullet.ScoreChanges -= ChangeScore;
    }
    
    public void StartGame()
    {
        ClearOldGame();

        player.gameObject.SetActive(true);
        player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        for (int i = 0; i < player.lives; i++)
        {
            Instantiate(lifePrefab, livesPanel.position, Quaternion.identity, livesPanel);
        }
        
        GameIsGoing = true;
        Time.timeScale = 1;
        SpawnWave();
        Invoke(nameof(SpawnUfo),  Random.Range(UfoSpawnTimeMin, UfoSpawnTimeMax));
    }

    private void ClearOldGame()
    {
        CancelInvoke();
        
        player.gameObject.SetActive(false);
        wave = 1;
        score = 0;
        scoreText.text = "Score: " + score;
        gameOverText.text = String.Empty;

        asteroidPool.pool.Clear();
        bulletPool.pool.Clear();
        
        for (int i = 0; i < livesPanel.childCount; i++)
        {
            Destroy(livesPanel.GetChild(i).gameObject);
        }
        for (int i = 0; i < asteroidPool.transform.childCount; i++)
        {
            Destroy(asteroidPool.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < bulletPool.transform.childCount; i++)
        {
            Destroy(bulletPool.transform.GetChild(i).gameObject);
        }
        foreach (var go in GameObject.FindGameObjectsWithTag("UFO"))
        {
            Destroy(go);
        }
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
        var trans = asteroidPool.pool.Get().GetComponent<Transform>();
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
