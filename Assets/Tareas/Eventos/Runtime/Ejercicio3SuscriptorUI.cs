using UnityEngine;

public class Ejercicio3SuscriptorUI : MonoBehaviour
{
    private void OnEnable()
    {
        Ejercicio3EventoEmisor.EventoInvocado += Reaccionar;
    }

    private void OnDisable()
    {
        Ejercicio3EventoEmisor.EventoInvocado -= Reaccionar;
    }

    private void Reaccionar()
    {
        PanelMensajesEventos.Publicar("UI reaccionó al evento.");
    }
}
