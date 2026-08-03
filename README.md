# MesaSitec

Mesa de servicio SaaS multi-tenant. Backend en .NET 8 (API REST) + Frontend en Vue 3.

## Requisitos previos

- .NET 8 SDK
- Node.js 18+ y npm

## Cómo levantar el proyecto

**1. Backend**

```bash
cd backend
export JWT_SECRET="MesaSitec-2026-Clave-Secreta-Super-Larga-Para-JWT-HS256-Desarrollo"
dotnet run --project src/Api
```

La API queda disponible en `http://localhost:5080`. Al arrancar, aplica las migraciones y siembra los datos automáticamente si la base está vacía (archivo `mesasitec.db`, se crea solo, sin pasos manuales).

Swagger disponible en `http://localhost:5080/swagger`.

**2. Frontend** (en otra terminal, sin cerrar la del backend)

```bash
cd frontend
npm install
npm run dev
```

Disponible en `http://localhost:5173`.

Probado de punta a punta: clonando el repositorio en una carpeta limpia y siguiendo únicamente estos pasos, el proyecto levanta correctamente en menos de 5 minutos.

## Variables de entorno

| Variable | Descripción | Default si no se define |
|---|---|---|
| `JWT_SECRET` | Clave para firmar los tokens JWT (HS256, mínimo 32 caracteres) | Falla al arrancar si no está definida |
| `SEED_FECHA_BASE` | Fecha base para generar los datos semilla de forma determinística | `2026-01-15T08:00:00Z` |

Ver `.env.example` en la raíz como referencia de los valores esperados (el backend lee estas variables del entorno del proceso, no de un archivo `.env` directamente).

## Credenciales de prueba

Contraseña para todos los usuarios semilla: `Sitec.2026`

| Email | Organización | Rol |
|---|---|---|
| admin@norte.test | Cooperativa Norte | Admin |
| agente1@norte.test | Cooperativa Norte | Agente |
| agente2@norte.test | Cooperativa Norte | Agente |
| user1@norte.test | Cooperativa Norte | Solicitante |
| user2@norte.test | Cooperativa Norte | Solicitante |
| admin@sur.test | Bufete Sur | Admin |
| user1@sur.test | Bufete Sur | Solicitante |

## Qué está implementado

- Los 9 endpoints del contrato, con las 7 reglas de negocio (RN-01 a RN-07)
- Aislamiento multi-tenant probado manualmente (404 cross-tenant)
- Autenticación JWT con los 4 claims requeridos
- Manejo de errores centralizado según el formato `problem+json` del contrato
- 10 pruebas unitarias con xUnit cubriendo máquina de estados, cálculo de SLA y permisos
- Frontend: las 5 vistas requeridas, con filtros/búsqueda/paginación server-side, y los `data-testid` de la sección 7.4
- Datos semilla determinísticos según `SEED_FECHA_BASE`
- Flujo probado en navegador: login, listado con filtros, crear/editar solicitud, ver detalle

## Qué no está completo / limitaciones conocidas

- El modal de "asignar" en el detalle de solicitud no tiene un selector real de agentes, porque el contrato no expone un endpoint de listado de usuarios (`GET /usuarios` no existe en la especificación). Queda como mejora pendiente.
- El toast de mensajes (`toast-mensaje`) existe en el DOM pero no está activado con lógica real todavía.
- No se generaron los DTOs del frontend desde OpenAPI automáticamente; se escribieron a mano en `frontend/src/types/dtos.ts`.
- No se implementó Docker Compose (era opcional).
- Las transiciones de estado (asignar/iniciar/resolver/cerrar/reabrir/cancelar) están implementadas y probadas en el backend, pero no se alcanzó a probar exhaustivamente cada una desde la interfaz web antes de la entrega.
