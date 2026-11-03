using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ejercicio1SaludoEmisor : MonoBehaviour
{
    public static event Action EventoSaludo;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Saludar();
        }
    }

    public void Saludar()
    {
        PanelMensajesEventos.Publicar("El emisor invocó EventoSaludo con ?.Invoke().");
        EventoSaludo?.Invoke();
    }

    private void OnDestroy()
    {
        EventoSaludo = null;
    }
}
