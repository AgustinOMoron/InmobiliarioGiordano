# InmobiliarioGiordano
# InmobiliarioGiordano

Aplicación de escritorio desarrollada en **Unity (C#)** para la gestión integral de **Inmobiliaria Luis Giordano**: propietarios, inquilinos, inmuebles, contratos de locación, liquidaciones, recibos y servicios, con persistencia en **Supabase** y actualización automática de alquileres según los índices oficiales del **BCRA** (ICL / CER).

## Funcionalidades principales

- **Gestión de propietarios e inquilinos** — alta, edición y consulta de perfiles (`DAODueno`, `DAOInquilino`).
- **Gestión de inmuebles y departamentos** — registro de propiedades, dirección, barrio y unidades (`DAOInmueble`, `DAODepartamento`).
- **Contratos de locación** — administración de contratos con monto, honorarios, periodicidad de actualización e índice aplicado (`DAOContrato`, `ContratoUI`).
- **Actualización automática de alquileres** — cálculo del nuevo monto según la variación de índices del BCRA entre dos fechas (`DAOIndicesAlquiler`):
  - **ICL** (Índice para Contratos de Locación, variable diaria del BCRA).
  - **CER**, usado como proxy del **IPC** para contratos indexados por inflación.
- **Liquidaciones a propietarios** — cálculo de honorarios, descuentos y neto a pagar, con generación de PDF (`DAOLiquidacion`, `LiquidacionUI`, `LiquidacionPDFGenerator`).
- **Recibos de pago** — emisión y descarga de recibos en PDF (`DAORecibo`, `ReciboUI`, `ReciboPDFGenerator`).
- **Servicios asociados a propiedades** — registro de gastos/servicios y su prorrateo (`DAOServicio`, `ServicioUI`).
- **Comunicación con inquilinos vía WhatsApp** — generación de mensajes prediseñados (recordatorio de pago, aviso de aumento) abiertos directamente a través de `wa.me` (`WhatsAppHelper`).
- **Autenticación de administrador** contra Supabase Auth (`DAOAdmin`).

## Stack técnico

| Componente | Detalle |
|---|---|
| Motor | Unity 6000.4.5f1 (Unity 6) |
| Lenguaje | C# |
| UI | Unity UI (uGUI) + TextMesh Pro |
| Backend / Base de datos | [Supabase](https://supabase.com) (REST + Auth), consumido vía `UnityWebRequest` |
| Generación de PDF | [QuestPDF](https://www.questpdf.com/) (licencia Community) |
| Índices económicos | API pública del [BCRA](https://www.bcra.gob.ar/) (`api.bcra.gob.ar/estadisticas/v4.0`) |
| Mensajería | WhatsApp (`wa.me`) |

## Arquitectura del proyecto

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

`BaseDAO` centraliza las llamadas HTTP a la API REST de Supabase (headers de autenticación, parseo de arrays JSON), y cada DAO concreto expone los métodos propios de su entidad, varios de ellos referenciando los requisitos funcionales del proyecto (`RF-01`, `RF-03`, `RF-04`, etc.) en sus comentarios.

## Requisitos

- **Unity Hub** con el editor **6000.4.5f1** (o la versión compatible que indique `ProjectSettings/ProjectVersion.txt`).
- Acceso a internet (consumo de Supabase y de la API del BCRA).
- Una instancia propia de **Supabase** con las tablas correspondientes a cada entidad (Dueño, Inquilino, Inmueble, Contrato, Liquidación, Recibo, Servicio, Admin).

## Configuración

Las credenciales de conexión a Supabase están centralizadas en:

```
Assets/Scripts/Configuraciones/SupabaseConfig.cs
```

> ⚠️ **Atención:** este archivo está versionado en el repositorio con la URL y la API key de Supabase escritas directamente en el código. Aunque se trate de la clave pública (`anon`) de Supabase —pensada para ser usada desde clientes— **se recomienda mover estos valores a variables de entorno o a un archivo no versionado** (agregado a `.gitignore`) antes de continuar el desarrollo o de publicar el repositorio, y rotar la clave si ya estuvo expuesta públicamente.

## Cómo abrir el proyecto

1. Clonar el repositorio:
   ```bash
   git clone https://github.com/AgustinOMoron/InmobiliarioGiordano.git
   ```
2. Abrir **Unity Hub** → *Add* → seleccionar la carpeta del proyecto clonado.
3. Abrir con la versión de Unity indicada en `ProjectSettings/ProjectVersion.txt`.
4. Verificar/completar `SupabaseConfig.cs` con las credenciales del proyecto de Supabase a utilizar.
5. Abrir la escena inicial `Assets/Scenes/Login.unity` y ejecutar.

## Estructura de escenas

- `Login.unity` — autenticación del administrador.
- `MenuPrincipal.unity` — punto de entrada a los distintos módulos de gestión.

## Scripts utilitarios (raíz del repositorio)

En la raíz del repo hay un conjunto de scripts en **Python** usados para explorar y validar manualmente las APIs públicas del BCRA y del INDEC durante el desarrollo del módulo de actualización de alquileres (no forman parte de la aplicación Unity en sí):

- `fetch_bcra.py` / `fetch_bcra_icl.py` — consultas de prueba a la API de variables monetarias del BCRA.
- `find_cer.py` — búsqueda del ID de variable correspondiente al CER dentro de `monetarias.json`.
- `test_dates.py` — prueba de disponibilidad de datos por rango de fechas.
- `test_indec.py` — consulta de prueba a la API de series de tiempo de datos.gob.ar (INDEC).

## Estado del proyecto

Proyecto en desarrollo activo, construido de forma incremental como TFG/proyecto aplicado a un caso real (Inmobiliaria Luis Giordano).

## Autoría

Desarrollado por **Agustín O. Morón** y **Jana Viciana**.
