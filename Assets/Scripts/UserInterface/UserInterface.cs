using UnityEngine;

public class UserInterface : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject GenerationOptions;
    public GameObject Preview;
    public GameObject GamePanel;
    public GameObject WorldGenerator;


    public GameObject Height;
    public GameObject Erosion;
    public GameObject PV;
    public GameObject Clouds;
    public GameObject WindY;
    public GameObject WindX;
    public GameObject WindXStrength;
    public GameObject WindYStrength;


    public State CurrentState;

    public Vector3 PreviewPositon { get; private set; } = new Vector3(256, 512, 256);
    public Quaternion PreviewRotation { get; private set; } =  Quaternion.Euler(90, 0, 0);

    void Start()
    {
        MainMenu.SetActive(true);
        GenerationOptions.SetActive(false);
    }

    void Update()
    {
        switch (CurrentState)
        {
            case State.MainMenu:
                MainMenu.SetActive(true);
                GenerationOptions.SetActive(false);
                Preview.SetActive(false);
                GamePanel.SetActive(false);
                break;
            case State.GenerationOptions:
                MainMenu.SetActive(false);
                Preview.SetActive(false);
                GenerationOptions.SetActive(true);
                GamePanel.SetActive(false);
                break;
            case State.Preview:
                Preview.SetActive(true);
                GamePanel.SetActive(false);
                GenerationOptions.SetActive(false);
                break;
            case State.Game:
                GamePanel.SetActive(true);
                MainMenu.SetActive(false);
                Preview.SetActive(false);
                GenerationOptions.SetActive(false);
                break;
        }
        HandleKeyPress();
    }

    private void HandleKeyPress()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            WorldGenerator.GetComponent<WorldGenerator>().RemovePlayer();
            CurrentState = State.GenerationOptions;
        }
    }

    public void GoBack(){
        CurrentState = State.GenerationOptions;
    }

    public void UseDefaultGeneration(){
        // var height = new NoiseSettings(0, 4, 100, 0.5f, 2, NoiseType.SIMPLEX);
        // var erosion = new NoiseSettings(0, 4, 100, 0.5f, 2, NoiseType.SIMPLEX);
        // var pv = new NoiseSettings(0, 4, 400, 0.5f, 2, NoiseType.SIMPLEX);
        // var winDirX = new NoiseSettings(0, 4, 100, 0.5f, 2, NoiseType.SIMPLEX);
        // var windDirY = new NoiseSettings(0, 4, 100, 0.5f, 2, NoiseType.SIMPLEX);
        // var windStrX = new NoiseSettings(0, 4, 100, 0.5f, 2, NoiseType.SIMPLEX);
        // var windStrY = new NoiseSettings(0, 4, 100, 0.5f, 2, NoiseType.SIMPLEX);
        // var clouds = new NoiseSettings(0, 4, 100, 0.5f, 2, NoiseType.SIMPLEX);

        var height = new NoiseSettings(12345, 6, 200, 0.5f, 2.0f, NoiseType.SIMPLEX);
        var erosion = new NoiseSettings(67890, 4, 150, 0.4f, 2.5f, NoiseType.SIMPLEX);
        var pv = new NoiseSettings(24680, 3, 400, 0.6f, 1.8f, NoiseType.SIMPLEX);
        var winDirX = new NoiseSettings(13579, 2, 100, 0.8f, 2.0f, NoiseType.SIMPLEX);
        var windDirY = new NoiseSettings(98765, 2, 100, 0.8f, 2.0f, NoiseType.SIMPLEX);
        var windStrX = new NoiseSettings(54321, 3, 50, 0.7f, 2.2f, NoiseType.SIMPLEX);
        var windStrY = new NoiseSettings(11223, 3, 50, 0.7f, 2.2f, NoiseType.SIMPLEX);
        var clouds = new NoiseSettings(78901, 5, 100, 0.6f, 1.9f, NoiseType.SIMPLEX);

        Height.GetComponent<SettingGatherer>().noiseSettings = height;
        Erosion.GetComponent<SettingGatherer>().noiseSettings = erosion;
        PV.GetComponent<SettingGatherer>().noiseSettings = pv;
        WindX.GetComponent<SettingGatherer>().noiseSettings = winDirX;
        WindY.GetComponent<SettingGatherer>().noiseSettings = windDirY;
        WindXStrength.GetComponent<SettingGatherer>().noiseSettings = windStrX;
        WindYStrength.GetComponent<SettingGatherer>().noiseSettings = windStrY;
        Clouds.GetComponent<SettingGatherer>().noiseSettings = clouds;
        StartGenerating();
    }

    public void UseUserSettingsGeneration(){
        
    }

    public void PlayGeneration(){
        CurrentState = State.Game;
        WorldGenerator.GetComponent<WorldGenerator>().PlacePlayer();
    }

    public void NewGeneration(){
        Height.GetComponent<SettingGatherer>().noiseSettings.seed = UnityEngine.Random.Range(0, 5000);
        Erosion.GetComponent<SettingGatherer>().noiseSettings.seed = UnityEngine.Random.Range(0, 5000);
        PV.GetComponent<SettingGatherer>().noiseSettings.seed = UnityEngine.Random.Range(0, 5000);
        WindX.GetComponent<SettingGatherer>().noiseSettings.seed = UnityEngine.Random.Range(0, 5000);
        WindY.GetComponent<SettingGatherer>().noiseSettings.seed = UnityEngine.Random.Range(0, 5000);
        WindXStrength.GetComponent<SettingGatherer>().noiseSettings.seed = UnityEngine.Random.Range(0, 5000);
        WindYStrength.GetComponent<SettingGatherer>().noiseSettings.seed = UnityEngine.Random.Range(0, 5000);
        Clouds.GetComponent<SettingGatherer>().noiseSettings.seed = UnityEngine.Random.Range(0, 5000);
        StartGenerating();
    }

    private void SetCamera(){
        var camera = GameObject.FindGameObjectWithTag("MainCamera");
        camera.transform.position = PreviewPositon;
        camera.transform.rotation = PreviewRotation;
    }

    public void StartGenerating()
    {
        CurrentState = State.Preview;
        SetCamera();

        var clouds = GameObject.FindGameObjectWithTag("Clouds").GetComponent<CloudGenerator>();
        clouds.SetSettings(Clouds.GetComponent<SettingGatherer>().noiseSettings);

        var wind = GameObject.FindGameObjectWithTag("Wind").GetComponent<WindGeneration>();
        wind.SetNoiseSettings(  WindX.GetComponent<SettingGatherer>().noiseSettings, 
                                WindY.GetComponent<SettingGatherer>().noiseSettings, 
                                WindXStrength.GetComponent<SettingGatherer>().noiseSettings, 
                                WindYStrength.GetComponent<SettingGatherer>().noiseSettings);

        WorldGenerator.GetComponent<WorldGenerator>().SetNoiseValues( Height.GetComponent<SettingGatherer>().noiseSettings,
                                                                      Erosion.GetComponent<SettingGatherer>().noiseSettings,
                                                                      PV.GetComponent<SettingGatherer>().noiseSettings
                                                                    );
           
        WorldGenerator.GetComponent<WorldGenerator>().StartGenerating();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        CurrentState = State.GenerationOptions;

    }

    public enum State
    {
        MainMenu,
        GenerationOptions,
        Preview,
        Game,
    }

}

