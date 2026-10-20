using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Monta a cena do jogo automaticamente na primeira vez que o projeto abre.
/// Também dá pra rodar de novo pelo menu Turno Seguro > Recriar cena.
/// </summary>
[InitializeOnLoad]
public static class ConfigurarProjeto
{
    const string ScenePath = "Assets/Scenes/Jogo.unity";

    static ConfigurarProjeto()
    {
        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(ScenePath)) CriarCena();
        };
    }

    [MenuItem("Turno Seguro/Recriar cena")]
    public static void CriarCena()
    {
        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cam.tag = "MainCamera";
        var c = cam.GetComponent<Camera>();
        c.orthographic = true;
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Theme.Background;
        cam.transform.position = new Vector3(0, 0, -10);

        var jogo = new GameObject("Jogo").AddComponent<GameUI>(); // GameManager vem junto pelo RequireComponent
        jogo.GetComponent<GameManager>().eventEveryRounds = 0;    // sem eventos aleatórios (a tela de evento fica dormente)

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        PlayerSettings.productName = "Turno Seguro";
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;

        AssetDatabase.SaveAssets();
        Debug.Log("Turno Seguro: cena criada em " + ScenePath + ". É só apertar Play.");
    }
}
