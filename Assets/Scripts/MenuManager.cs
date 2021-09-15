using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR                
using UnityEditor;
#endif

public class MenuManager : MonoBehaviour {
	
	private static MenuManager _instance;
	public static MenuManager Instance => _instance;

	public bool UseMouse { get; private set; }
	public bool Paused { get; private set; } = true;
	
	[SerializeField] private GameObject menuPanel;
	[SerializeField] private Button controlButton;
	[SerializeField] private GameObject continueButtonPanel;

	
	private void Awake()
	{
		if (_instance == null) _instance = this;
	}
	private void Start()
	{
		continueButtonPanel.SetActive(false);
	}
	
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))             
		{
			Pause();                                         
		}
	}

	public void NewGame()
	{
		Paused = false;
		menuPanel.SetActive(false);
		GameManager.Instance.StartGame();
	}
	
	public void Pause()
	{
		continueButtonPanel.SetActive(GameManager.Instance.GameIsGoing);
		Paused = !menuPanel.activeSelf;
		menuPanel.SetActive(Paused);
		Time.timeScale = Time.timeScale == 0 ? 1 : 0;
	}
	
	public void ChangeControl()
	{
		UseMouse = !UseMouse;
		controlButton.GetComponentInChildren<TextMeshProUGUI>().text =
			UseMouse ? "Control: keyboard only" : "Control: keyboard+mouse";
	}

	public void Quit()                                      
    {
		#if UNITY_EDITOR 
		EditorApplication.isPlaying = false;
		#else 
		Application.Quit();
		#endif
	}
}
