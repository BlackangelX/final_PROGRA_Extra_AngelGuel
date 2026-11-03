using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ejercicio3EventoEmisor : MonoBehaviour
{
    public static event Action EventoInvocado;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            InvocarEvento();
        }
    }

    public void InvocarEvento()
    {
        PanelMensajesEventos.Publicar("El emisor invocó el evento.");
        EventoInvocado?.Invoke();
    }

    private void OnDestroy()
    {
        EventoInvocado = null;
    }
}
