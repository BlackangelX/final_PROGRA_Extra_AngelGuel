# Proyecto final multijugador

Proyecto de Unity con Photon Fusion para crear, visualizar y unirse a sesiones multijugador.

## Funcionalidades

- Lista dinámica de sesiones públicas disponibles.
- Unión directa a una sesión seleccionada o mediante su nombre.
- Creación de sesiones con nombre personalizado.
- Selección de 2 a 10 jugadores; el valor predeterminado es 2.
- Posiciones de aparición independientes para hasta 10 participantes.
- Equipos equilibrados para partidas de más de dos jugadores.
- Mensajes de carga al conectarse al lobby de Photon o a una sesión.
- Partidas privadas accesibles mediante su nombre.

## Requisitos

- Unity 6000.1.1f1.
- Conexión a Internet para Photon Fusion.

## Prueba recomendada

1. Ejecutar dos copias del build.
2. En la primera, crear una sesión pública con un nombre y el máximo deseado.
3. En la segunda, abrir **Servidores** y seleccionar la sesión.
4. Para probar más jugadores, abrir copias adicionales hasta alcanzar el límite configurado.

## Controles

- Movimiento: WASD.
- Cámara: mouse.
- Correr, disparar y ataque cuerpo a cuerpo: controles configurados en el proyecto.

La escena incluida en el build es `Assets/Scenes/SampleScene.unity`.
