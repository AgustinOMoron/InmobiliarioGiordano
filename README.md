# 🏢 InmobiliarioGiordano

Aplicación de escritorio desarrollada en **Unity (C#)** para la gestión integral de la **Inmobiliaria Luis Giordano**: propietarios, inquilinos, inmuebles, contratos de locación, liquidaciones, recibos y servicios. Incluye actualización automática de alquileres según los índices oficiales del **BCRA** (ICL / CER) y persistencia de datos en **Supabase**.

---

## 📋 Descripción general

InmobiliarioGiordano digitaliza la operación administrativa de una inmobiliaria real, automatizando uno de los procesos más sensibles del negocio: el cálculo de aumentos de alquiler según los índices oficiales vigentes en Argentina. El sistema centraliza la gestión de propietarios, inquilinos, contratos y liquidaciones en un panel de control único, con generación de comprobantes en PDF y comunicación directa con los inquilinos por WhatsApp.

## ✨ Funcionalidades principales

- **Gestión de propietarios e inquilinos** — alta, edición y consulta de perfiles.
- **Gestión de inmuebles y departamentos** — registro de propiedades, dirección, barrio y unidades.
- **Contratos de locación** — administración de contratos con monto, honorarios, periodicidad de actualización e índice aplicado.
- **Actualización automática de alquileres** — cálculo del nuevo monto según la variación de índices del BCRA entre dos fechas:
  - **ICL** (Índice para Contratos de Locación, variable diaria del BCRA).
  - **CER**, usado como proxy del **IPC** para contratos indexados por inflación.
- **Liquidaciones a propietarios** — cálculo de honorarios, descuentos y neto a pagar, con generación de PDF.
- **Recibos de pago** — emisión y descarga de recibos en PDF.
- **Servicios asociados a propiedades** — registro de gastos/servicios y su prorrateo.
- **Comunicación con inquilinos vía WhatsApp** — mensajes prediseñados (recordatorio de pago, aviso de aumento) enviados directamente a través de `wa.me`.
- **Autenticación de administrador** contra Supabase Auth.

## 🛠️ Stack técnico

| Componente               | Detalle                                                                     |
|----------------------------|------------------------------------------------------------------------------|
| Motor                      | Unity 6000.4.5f1 (Unity 6)                                                   |
| Lenguaje                   | C#                                                                           |
| UI                         | Unity UI (uGUI) + TextMesh Pro                                              |
| Backend / Base de datos    | [Supabase](https://supabase.com) (REST + Auth), consumido vía `UnityWebRequest` |
| Generación de PDF          | [QuestPDF](https://www.questpdf.com/) (licencia Community)                  |
| Índices económicos         | API pública del [BCRA](https://www.bcra.gob.ar/) (`api.bcra.gob.ar/estadisticas/v4.0`) |
| Mensajería                 | WhatsApp (`wa.me`)                                                          |

## 📁 Arquitectura del proyecto

El código fuente vive en `Assets/Scripts`, organizado por capas:

```
Assets/Scripts/
├── Configuraciones/   # Configuración de Supabase, cambio de escenas, menú
├── DAO/                # Acceso a datos: un DAO por entidad (Dueño, Inquilino,
│                       # Inmueble, Contrato, Liquidación, Recibo, Servicio, Admin)
│                       # heredando de BaseDAO (GET/POST/PATCH/DELETE genéricos)
├── Interfaz/           # Controladores de UI por pantalla/módulo
├── Modelos/            # DTOs para reportes (Liquidación, Recibo)
├── Servicios/          # Generación de PDF (QuestPDF) y mensajes de WhatsApp
└── Animacion/          # Utilidades de animación de UI
```

`BaseDAO` centraliza las llamadas HTTP a la API REST de Supabase (headers de autenticación, parseo de arrays JSON), y cada DAO concreto expone los métodos propios de su entidad, varios de ellos vinculados a requisitos funcionales del proyecto (`RF-01`, `RF-03`, `RF-04`, etc.).

## 🚀 Cómo abrir el proyecto

### Requisitos

- **Unity Hub** con el editor **6000.4.5f1** (o la versión compatible indicada en `ProjectSettings/ProjectVersion.txt`).
- Acceso a internet (consumo de Supabase y de la API del BCRA).
- Una instancia propia de **Supabase** con las tablas correspondientes a cada entidad (Dueño, Inquilino, Inmueble, Contrato, Liquidación, Recibo, Servicio, Admin).

### Pasos

1. Cloná el repositorio:
   ```bash
   git clone https://github.com/AgustinOMoron/InmobiliarioGiordano.git
   ```

2. Abrí **Unity Hub** → *Add* → seleccioná la carpeta del proyecto clonado.

3. Abrí el proyecto con la versión de Unity indicada en `ProjectSettings/ProjectVersion.txt`.

4. Completá `Assets/Scripts/Configuraciones/SupabaseConfig.cs` con las credenciales de tu propio proyecto de Supabase.

5. Abrí la escena inicial `Assets/Scenes/Login.unity` y ejecutá.

> ⚠️ **Nota de seguridad:** las credenciales de conexión a Supabase no deberían quedar hardcodeadas ni versionadas en el código. Se recomienda moverlas a variables de entorno o a un archivo no versionado (agregado a `.gitignore`), y rotar la clave si estuvo expuesta públicamente.

## 🗺️ Estructura de escenas

- `Login.unity` — autenticación del administrador.
- `MenuPrincipal.unity` — punto de entrada a los distintos módulos de gestión.

## 🐍 Scripts utilitarios (raíz del repositorio)

En la raíz del repo hay un conjunto de scripts en **Python** usados para explorar y validar manualmente las APIs públicas del BCRA y del INDEC durante el desarrollo del módulo de actualización de alquileres (no forman parte de la aplicación Unity en sí):

- `fetch_bcra.py` / `fetch_bcra_icl.py` — consultas de prueba a la API de variables monetarias del BCRA.
- `find_cer.py` — búsqueda del ID de variable correspondiente al CER dentro de `monetarias.json`.
- `test_dates.py` — prueba de disponibilidad de datos por rango de fechas.
- `test_indec.py` — consulta de prueba a la API de series de tiempo de datos.gob.ar (INDEC).

## 📌 Estado del proyecto

Proyecto en desarrollo activo, construido de forma incremental como Trabajo Final de Grado (TFG) aplicado a un caso real: la Inmobiliaria Luis Giordano.

## 👥 Autores

- [Jana Viciana](https://github.com/JanaV8)
- [Agustín Omar Morón](https://github.com/AgustinOMoron)
- [Maximo Martinez](https://github.com/MaximoAMartinez))
