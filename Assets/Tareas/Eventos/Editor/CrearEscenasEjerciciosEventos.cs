using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CrearEscenasEjerciciosEventos
{
    private const string ScenesFolder = "Assets/Tareas/Eventos/Scenes";
    private const string TargetScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ExerciseScenePath = ScenesFolder + "/Ejercicios_Eventos.unity";
    private const string EventRootName = "TAREA EVENTOS - event Action";

    private static readonly string[] OldScenePaths =
    {
        ScenesFolder + "/Ejercicio1_Saludo.unity",
        ScenesFolder + "/Ejercicio2_SuscripcionDinamica.unity",
        ScenesFolder + "/Ejercicio3_ActivarSuscriptores.unity"
    };

    [MenuItem("Tareas/Eventos/Crear escena independiente de ejercicios")]
    public static void BuildScenes()
    {
        EditorSceneManager.SaveOpenScenes();
        Scene sampleScene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);

        GameObject grupoAnterior = sampleScene.GetRootGameObjects()
            .FirstOrDefault(objeto => objeto.name == EventRootName);
        if (grupoAnterior != null)
        {
            Object.DestroyImmediate(grupoAnterior);
            EditorSceneManager.MarkSceneDirty(sampleScene);
            EditorSceneManager.SaveScene(sampleScene, TargetScenePath);
        }

        Scene escenaEjercicios = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        escenaEjercicios.name = "Ejercicios_Eventos";
        CrearCamara();

        GameObject ejercicio1 = CrearGrupo(
            "EJERCICIO 1 - Saludo con evento",
            "EJERCICIO 1 - SALUDO CON EVENTO",
            "Presiona ESPACIO. El emisor usa event Action y ?.Invoke(); el suscriptor muestra el saludo.",
            typeof(Ejercicio1SaludoEmisor),
            typeof(Ejercicio1SaludoSuscriptor));

        GameObject ejercicio2 = CrearGrupo(
            "EJERCICIO 2 - Suscripción dinámica",
            "EJERCICIO 2 - SUSCRIPCIÓN DINÁMICA",
            "Presiona ESPACIO dos veces. Primero reaccionan A y B; B se desuscribe con -= y después solo reacciona A.",
            typeof(Ejercicio2EventoEmisor),
            typeof(Ejercicio2SuscriptorA),
            typeof(Ejercicio2SuscriptorB));

        GameObject ejercicio3 = CrearGrupo(
            "EJERCICIO 3 - Activar y desactivar suscriptores",
            "EJERCICIO 3 - ACTIVAR Y DESACTIVAR SUSCRIPTORES",
            "Presiona ESPACIO. En Play, desactiva y reactiva uno de los tres componentes suscriptores desde el Inspector.",
            typeof(Ejercicio3EventoEmisor),
            typeof(Ejercicio3SuscriptorLuz),
            typeof(Ejercicio3SuscriptorSonido),
            typeof(Ejercicio3SuscriptorUI));
        ejercicio1.SetActive(true);
        ejercicio2.SetActive(false);
        ejercicio3.SetActive(false);

        EditorSceneManager.SaveScene(escenaEjercicios, ExerciseScenePath);
        ActualizarBuildSettings();
        EliminarEscenasAnteriores();
        Selection.activeGameObject = ejercicio1;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Se creó la escena independiente Ejercicios_Eventos y se limpió SampleScene.");
    }

    private static void CrearCamara()
    {
        GameObject camaraObject = new GameObject("Main Camera");
        Camera camara = camaraObject.AddComponent<Camera>();
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.035f, 0.045f, 0.075f);
        camaraObject.tag = "MainCamera";
        camaraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static GameObject CrearGrupo(
        string nombre,
        string titulo,
        string instrucciones,
        params System.Type[] componentes)
    {
        GameObject ejercicio = new GameObject(nombre);
        foreach (System.Type componente in componentes)
        {
            ejercicio.AddComponent(componente);
        }

        GameObject panelObject = new GameObject("Panel de instrucciones y mensajes");
        panelObject.transform.SetParent(ejercicio.transform);
        PanelMensajesEventos panel = panelObject.AddComponent<PanelMensajesEventos>();
        panel.Configurar(titulo, instrucciones);
        return ejercicio;
    }

    private static void ActualizarBuildSettings()
    {
        List<EditorBuildSettingsScene> escenas = EditorBuildSettings.scenes
            .Where(escena => !OldScenePaths.Contains(escena.path))
            .ToList();

        if (escenas.All(escena => escena.path != ExerciseScenePath))
        {
            escenas.Add(new EditorBuildSettingsScene(ExerciseScenePath, true));
        }

        EditorBuildSettings.scenes = escenas.ToArray();
    }

    private static void EliminarEscenasAnteriores()
    {
        foreach (string ruta in OldScenePaths)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ruta) != null)
            {
                AssetDatabase.DeleteAsset(ruta);
            }
        }
    }
}

[InitializeOnLoad]
public static class IntegrarEventosUnaVez
{
    private const string IntegrationKey = "Codex.EventosAction.EscenaIndependiente.20261009";

    static IntegrarEventosUnaVez()
    {
        if (!EditorPrefs.GetBool(IntegrationKey, false))
        {
            EditorApplication.delayCall += Integrar;
        }
    }

    private static void Integrar()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Integrar;
            return;
        }

        try
        {
            CrearEscenasEjerciciosEventos.BuildScenes();
            EditorPrefs.SetBool(IntegrationKey, true);
        }
        catch (System.Exception excepcion)
        {
            Debug.LogException(excepcion);
        }
    }
}
