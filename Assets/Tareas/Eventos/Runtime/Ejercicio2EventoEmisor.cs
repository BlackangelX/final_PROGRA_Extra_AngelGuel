using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Ejercicio2EventoEmisor : MonoBehaviour
{
    public static event Action EventoDinamico;
    private int numeroInvocacion;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            InvocarEvento();
        }
    }

    public void InvocarEvento()
    {
        numeroInvocacion++;
        PanelMensajesEventos.Publicar($"Invocación {numeroInvocacion} del evento dinámico.");
        EventoDinamico?.Invoke();
    }

    private void OnDestroy()
    {
        EventoDinamico = null;
    }
}
