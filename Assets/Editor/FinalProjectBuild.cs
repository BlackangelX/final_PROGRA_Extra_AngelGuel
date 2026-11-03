using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class FinalProjectBuild
{
    public static void BuildWindows()
    {
        string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
        string outputDirectory = Path.Combine(projectRoot, "Builds", "Windows");
        string executablePath = Path.Combine(outputDirectory, "ProyectoFinalMultijugador.exe");
        Directory.CreateDirectory(outputDirectory);

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new InvalidOperationException("No hay escenas habilitadas en Build Settings.");

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = executablePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException(
                $"El build falló: {report.summary.result}, {report.summary.totalErrors} errores.");

        UnityEngine.Debug.Log(
            $"BUILD_FINAL_SUCCESS: {executablePath} ({report.summary.totalSize} bytes)");
    }
}
