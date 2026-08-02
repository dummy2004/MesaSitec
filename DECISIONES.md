# Decisiones técnicas — MesaSitec

## Tres decisiones técnicas

**1. Arquitectura en capas (Api / Aplicacion / Dominio / Infraestructura) en vez de todo en un solo proyecto.**
Alternativa descartada: un único proyecto Web API con todo junto. Se separó porque la máquina de estados, el cálculo de SLA y los permisos necesitaban ser testeables con xUnit sin levantar un servidor HTTP completo — cumple el requisito de la sección 5.2 de que la lógica de negocio no viva dentro de los controllers.

**2. GUID como identificador en vez de int autoincremental.**
Alternativa descartada: IDs numéricos secuenciales. Se usó GUID porque el modelo de datos del enunciado lo pedía así explícitamente, y además evita que un ID en la URL revele cuántos registros existen en el sistema (relevante en un contexto multi-tenant).

**3. Excepciones tipadas de dominio (`DominioException` y sus 8 subclases) capturadas por un middleware global, en vez de manejar cada error dentro de cada controller.**
Alternativa descartada: `try/catch` individual en cada acción de cada controller, devolviendo el JSON de error a mano cada vez. Se centralizó en un middleware porque el contrato exige un formato `problem+json` idéntico en todos los errores (sección 6.1); con excepciones tipadas, el dominio solo lanza el error con su código de negocio y el middleware decide el HTTP status, sin acoplar la lógica de negocio a conceptos HTTP.

## Qué se hizo con ayuda de IA y qué a mano

Se usó Claude para generar la estructura inicial del proyecto, el código de las 4 capas del backend, los controllers, el frontend en Vue, y para depurar errores de compilación y de configuración de entorno (versiones incompatibles de paquetes NuGet, configuración de EF Core Tools, CORS, JWT). El código fue revisado y explicado línea por línea durante el desarrollo para asegurar comprensión completa antes de la entrega. La configuración manual del entorno (instalación de SDKs, Git, resolución de rutas de carpetas, variables de entorno) se hizo directamente por el desarrollador, con guía de la IA.

## Qué haría distinto con una semana más

- Agregaría un endpoint `GET /usuarios` (o al menos `GET /usuarios?rol=Agente`) para poder implementar un selector real de agentes en el modal de asignación del frontend, en vez de dejarlo como limitación conocida.
- Generaría los tipos TypeScript del frontend automáticamente desde el esquema OpenAPI de Swagger, en vez de escribirlos a mano, para eliminar el riesgo de que se desincronicen del contrato real.
- Agregaría más pruebas de integración (no solo unitarias) que levanten la API completa con una base SQLite en memoria y prueben los 9 endpoints de punta a punta.
- Implementaría el sistema de notificaciones (`toast-mensaje`) con lógica real de éxito/error en cada acción del frontend.
- Agregaría Docker Compose para no depender de tener instalado el SDK de .NET y Node localmente.

## Dónde me atasqué y cómo lo resolví

El mayor atasco fue un bug real en el cálculo del correlativo del código de solicitud (RN-07): el controller de creación calculaba el correlativo contando cuántas solicitudes existían con `FechaCreacion` en el año actual, pero los datos semilla incluyen solicitudes con fecha movida artificialmente a diciembre del año anterior (para poder demostrar el caso de "solicitud vencida" desde el arranque). Esto causaba que el conteo diera un número menor al real, generando un código que ya existía en la base — un error `UNIQUE constraint failed` al crear una nueva solicitud desde el frontend. Se solucionó cambiando el criterio de conteo de "por fecha de creación" a "por prefijo del código" (`SOL-{año}-`), que refleja la cantidad real de códigos usados independientemente de la fecha real de cada registro.

Un atasco secundario, más de entorno que de código: varios paquetes NuGet (EF Core, JwtBearer, Swashbuckle) instalaban por defecto su versión más reciente (10.x), incompatible con el `TargetFramework net8.0` del proyecto. Se resolvió fijando explícitamente las versiones 8.x compatibles en cada `dotnet add package`.