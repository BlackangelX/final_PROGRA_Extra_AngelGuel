# TAREA 04 - Login & Create Account

Escena: `Assets/Tareas/LoginCreateAccount/Scenes/Tarea04_LoginCreateAccount.unity`

## Contenido

- Panel de inicio de sesión con correo, contraseña y navegación a Crear Cuenta.
- Panel de creación de cuenta con correo, usuario, contraseña y navegación a Login.
- Registro real mediante `PlayFabClientAPI.RegisterPlayFabUser`.
- Inicio de sesión real mediante `PlayFabClientAPI.LoginWithEmailAddress`.
- Conversión de callbacks a tareas con `TaskCompletionSource` y consumo mediante `async/await`.
- Canvas configurado con `Scale With Screen Size` y resolución de referencia 1920 x 1080.

## Configuración necesaria

Antes de probar las llamadas reales, selecciona el objeto `PlayFab Login Controller` de la escena y escribe el `Title Id` de tu título de PlayFab en el Inspector. También se puede configurar globalmente en `PlayFabSharedSettings`.

Para reconstruir la escena desde el editor usa:

`Tareas > Login & Create Account > Construir escena`
