using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class Tarea04StandaloneBuild
{
    private const string ScenePath = "Assets/Tareas/LoginCreateAccount/Scenes/Tarea04_LoginCreateAccount.unity";

    public static void BuildWindows()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputDirectory = Path.Combine(projectRoot, "Builds", "Tarea04");
        string executablePath = Path.Combine(outputDirectory, "Tarea04_LoginCreateAccount.exe");
        Directory.CreateDirectory(outputDirectory);

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = executablePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"El build de la Tarea 04 falló: {report.summary.result}, {report.summary.totalErrors} errores.");
        }

        Debug.Log($"TAREA04_BUILD_WINDOWS_SUCCESS: {executablePath} ({report.summary.totalSize} bytes)");
    }
}
