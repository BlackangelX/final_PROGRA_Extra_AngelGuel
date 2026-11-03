using UnityEngine;

public class Ejercicio3SuscriptorSonido : MonoBehaviour
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
        PanelMensajesEventos.Publicar("Sonido reaccionó al evento.");
    }
}
