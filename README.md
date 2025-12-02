# 💈 BarberShopApp: Sistema de Gestión de Citas y Multi-Tenancy

Este es el repositorio del proyecto **BarberShopApp**, una aplicación web construida con **ASP.NET Core Razor Pages** que simula un sistema de gestión de citas para una cadena de barberías.

El proyecto implementa funcionalidades clave como la **Arquitectura Multi-Tenancy** (Múltiples Clientes/Barberías en una sola base de datos), gestión de usuarios con ASP.NET Identity, y lógica de validación de reservas.

## 🛠️ Tecnologías Utilizadas

* **Backend:** ASP.NET Core 7/8 (Razor Pages)
* **Base de Datos:** SQL Server (con Entity Framework Core)
* **Gestión de Datos:** Arquitectura Multi-Tenancy (filtrado por `TenantId`).
* **Autenticación:** ASP.NET Core Identity.
* **Notificaciones:** Servicio de Correo (SMTP) para confirmaciones de citas.
* **Frontend:** HTML, CSS (Bootstrap 5).

## 🚀 Funcionalidades Clave Implementadas

| Funcionalidad | Descripción | Estado |
| :--- | :--- | :--- |
| **Multi-Tenancy** | Filtrado de datos (Citas, Barberos, Servicios) por TenantID. | ✅ Implementado |
| **CRUD de Barberos** | Creación, lectura, actualización y borrado de barberos. | ✅ Implementado |
| **CRUD de Servicios** | Creación, lectura, actualización y borrado de servicios. | ✅ Implementado |
| **Creación de Citas** | Formulario para que el cliente reserve una cita. | ✅ Implementado |
| **Validación de Disponibilidad** | Lógica para evitar la superposición de citas para un mismo barbero. | ✅ Implementado |
| **Notificaciones por Correo** | Envío automático de confirmación de cita al cliente y al administrador. | ✅ Implementado |
| **Página de Confirmación** | Muestra el resumen de la cita exitosa. | ✅ Implementado |

## ⚙️ Configuración del Entorno

Sigue estos pasos para levantar el proyecto localmente:

### 1. Clonar el Repositorio

```bash
git clone [https://www.youtube.com/watch?v=44ziZ12rJwU](https://www.youtube.com/watch?v=44ziZ12rJwU)
cd BarberShopApp
2. Configurar la Base de Datos
El proyecto utiliza Entity Framework Core.

Abre la Consola del Administrador de Paquetes (Package Manager Console) en Visual Studio.

Aplica las migraciones existentes:

PowerShell

Update-Database
3. Configurar Servicios Externos (SMTP)
Debes configurar tus credenciales de correo electrónico en el archivo appsettings.json (o appsettings.Development.json) para que las notificaciones automáticas funcionen:

JSON

"EmailSettings": {
    "SmtpHost": "smtp.ejemplo.com", 
    "SmtpPort": 587,
    "SmtpUser": "tu_correo@dominio.com",
    "SmtpPass": "tu_contraseña_o_app_password",
    "SenderEmail": "tu_correo@dominio.com",
    "BarberEmail": "admin@barbershop.com"
}
Nota: Si usas un proveedor como Gmail, asegúrate de generar una "App Password" en lugar de usar tu contraseña principal.

4. Inicialización
Una vez configurado, ejecuta el proyecto (F5 en Visual Studio) para iniciar sesión con un usuario y probar el sistema de Multi-Tenancy.

🤝 Contribuciones
Si deseas contribuir, por favor sigue estos pasos:

Haz un "Fork" del repositorio.

Crea una nueva rama para tu funcionalidad (git checkout -b feature/nueva-funcionalidad).

Realiza tus cambios.

Asegúrate de que el código pase todas las validaciones.

Crea un Pull Request.

📝 Licencia
Este proyecto está bajo la Licencia MIT.
