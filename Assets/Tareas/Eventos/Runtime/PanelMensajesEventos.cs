using System.Collections.Generic;
using UnityEngine;

public class PanelMensajesEventos : MonoBehaviour
{
    private static PanelMensajesEventos instancia;
    private readonly List<string> mensajes = new List<string>();
    private string titulo = "Ejercicio de eventos";
    private string instrucciones = "Presiona ESPACIO para invocar el evento.";
    private Font fuenteTitulos;
    private Font fuenteTexto;

    public void Configurar(string nuevoTitulo, string nuevasInstrucciones)
    {
        titulo = nuevoTitulo;
        instrucciones = nuevasInstrucciones;
    }

    public static void Publicar(string mensaje)
    {
        Debug.Log(mensaje);

        if (instancia == null)
        {
            return;
        }

        instancia.mensajes.Add(mensaje);
        if (instancia.mensajes.Count > 12)
        {
            instancia.mensajes.RemoveAt(0);
        }
    }

    private void Awake()
    {
        instancia = this;
        fuenteTitulos = Resources.Load<Font>("Fonts/Retrolab");
        fuenteTexto = Resources.Load<Font>("Fonts/Bahnschrift");
    }

    private void OnDestroy()
    {
        if (instancia == this)
        {
            instancia = null;
        }
    }

    private void OnGUI()
    {
        float ancho = Mathf.Min(760f, Screen.width - 40f);
        GUILayout.BeginArea(new Rect(20f, 20f, ancho, Screen.height - 40f), GUI.skin.box);

        GUIStyle tituloStyle = new GUIStyle(GUI.skin.label)
        {
            font = fuenteTitulos,
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            normal = { textColor = new Color(0.98f, 0.78f, 0.22f) }
        };

        GUIStyle textoStyle = new GUIStyle(GUI.skin.label)
        {
            font = fuenteTexto,
            fontSize = 18,
            wordWrap = true,
            normal = { textColor = Color.white }
        };

        GUILayout.Label(titulo, tituloStyle);
        GUILayout.Space(8f);
        GUILayout.Label(instrucciones, textoStyle);
        GUILayout.Space(18f);
        GUILayout.Label("Mensajes del evento:", tituloStyle);

        if (mensajes.Count == 0)
        {
            GUILayout.Label("Todavía no se ha invocado el evento.", textoStyle);
        }
        else
        {
            foreach (string mensaje in mensajes)
            {
                GUILayout.Label("• " + mensaje, textoStyle);
            }
        }

        GUILayout.EndArea();
    }
}
