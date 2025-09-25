# Comments Sentiment API

API para gestionar comentarios de productos y analizar su sentimiento de manera automática.

---
## Opción 1: Ejecución con Docker

### 1) Clonar el repositorio

```bash
git clone https://github.com/ElMike3D/comments-sentiment-api.git
cd comments-sentiment-api
```

### 2) Requisitos

* [Docker Desktop](https://www.docker.com/products/docker-desktop)
* Docker Compose

### 3) Configurar variables de entorno

Crea un archivo `.env` en la raíz del repositorio con:

```
DB_CONNECTION=Server=sentiment-sqlserver,1433;Database=SentimentDb;User Id=test_admin;Password=Password123!;TrustServerCertificate=True;
GEMINI_API_KEY=TU_API_KEY
```

### 4) Levantar los contenedores

```bash
docker compose up --build
```

* Esto levantará:

  * SQL Server con base `SentimentDb` inicializada
  * API `SentimentApi` escuchando en `http://localhost:5000`

### 5) Acceder a la API

* HTTP: `http://localhost:5000`
* Swagger UI: `http://localhost:5000/swagger`

### 6) Notas

* La db y los datos iniciales de prueba se cargan automáticamente gracias al contenedor `db-init`.
* Si quieres reiniciar todo, elimina el volumen de SQL Server:

```bash
docker volume rm comments-sentiment-api_sqlserver-data
```

## Opción 2: Ejecución Local (sin Docker)

### 1) Clonar el repositorio

```bash
git clone https://github.com/ElMike3D/comments-sentiment-api.git
cd comments-sentiment-api/SentimentApi
```

### 2) Requisitos

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* SQL Server accesible desde tu equipo

### 3) Inicializar la base de datos

* Ejecuta el script `db-init/init.sql`:

  * Con **SQL Server Management Studio (SSMS)**: abre y ejecuta el archivo.
  * Con **sqlcmd** (Windows):

```bash
sqlcmd -S localhost -d master -i "db-init\init.sql" -E
```

* La base creada es `SentimentDb` e incluye la tabla `Comments` con datos de prueba insertados.

### 4) Configurar conexión y API Key

Crea un archivo `.env` en `SentimentApi/` con:

```
DB_CONNECTION=Server=localhost;Database=SentimentDb;User Id=test_admin;Password=Password123;TrustServerCertificate=True;
GEMINI_API_KEY=TU_API_KEY
PORT=5019
```

### 5) Construir y ejecutar la aplicación

```bash
dotnet restore
dotnet run
```

### 6) Acceder a la API

* HTTP: `http://localhost:5019`
* HTTPS: `https://localhost:7015`
* Swagger UI: `http://localhost:5019/swagger`

### 7) Pruebas rápidas

```bash
# Verificar conexión a la base de datos
curl http://localhost:5019/api/comments/ping

# Obtener todos los comentarios
curl http://localhost:5019/api/comments
```

---


## Troubleshooting

### Error de conexión a SQL Server (Windows)

Si obtienes errores de conexión a la base de datos como:
- `Login failed for user`
- `A network-related or instance-specific error occurred`

**Problema común:** SQL Server en Windows puede tener configurado solo autenticación de Windows por defecto.

**Solución:**
1. Abre **SQL Server Management Studio (SSMS)** como administrador
2. Conéctate a tu instancia de SQL Server
3. Click derecho en el servidor → **Properties**
4. Ve a **Security**
5. En **Server authentication**, selecciona **SQL Server and Windows Authentication mode**
6. Click **OK**
7. **Reinicia el servicio de SQL Server:**
   - Abre **Services** (`services.msc`)
   - Busca tu instancia de SQL Server (ej: `SQL Server (MSSQLSERVER)`)
   - Click derecho → **Restart**

### Otros errores comunes

- **Puerto HTTPS no disponible:** Si ves `Failed to determine the https port for redirect`, usa solo HTTP: `http://localhost:5019`
- **Base de datos no existe:** Asegúrate de haber ejecutado el script `db-init/init.sql`

## Base de datos (referencia rápida)

- Base de datos: `SentimentDb`
- Tabla: `Comments`

```sql
CREATE TABLE Comments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId NVARCHAR(50) NOT NULL,
    UserId NVARCHAR(50) NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    Sentiment NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
```

- Inserciones de ejemplo:

```sql
INSERT INTO Comments (ProductId, UserId, CommentText, Sentiment) VALUES
('PROD001','USER001','Este producto es excelente, superó mis expectativas.','positivo'),
('PROD002','USER002','El producto tiene un defecto y es terrible.','negativo'),
('PROD003','USER003','Me gustó y a la vez tiene algunos problemas.','neutral');
```

---

## Endpoints

### 1. Ping

- **GET** `/api/comments/ping`
- Prueba la conexión con la base de datos.
- **Response:**
```json
"Conexión a la base de datos exitosa"
```

---

### 2. Obtener comentario por ID

- **GET** `/api/comments/{id}`
- Obtiene un comentario específico.
- **Response:**
```json
{
  "id": 1,
  "productId": "PROD001",
  "userId": "USER001",
  "commentText": "Este producto es excelente",
  "sentiment": "positivo",
  "createdAt": "2025-09-24T18:00:00Z"
}
```

---

### 3. Crear comentario manual

- **POST** `/api/comments`
- **Body** (`JSON`):
```json
{
  "productId": "PROD001",
  "userId": "USER001",
  "commentText": "Excelente producto, muy recomendable"
}
```
- La API determina automáticamente el `sentiment` según palabras clave.
- **Response:** `201 Created` con el comentario creado.

---

### 4. Listar comentarios con filtros

- **GET** `/api/comments`
- **Query Params opcionales:**
  - `product_id`: filtra por producto.
  - `sentiment`: filtra por sentimiento (`positivo`, `negativo`, `neutral`).
- **Response:**
```json
[
  {
    "id": 1,
    "productId": "PROD001",
    "userId": "USER001",
    "commentText": "Excelente producto",
    "sentiment": "positivo",
    "createdAt": "2025-09-24T18:00:00Z"
  }
]
```

---

### 5. Resumen de sentimientos de todos los comentarios

- **GET** `/api/sentiment-summary`
- **Response:**
```json
{
  "total_comments": 10,
  "sentiment_counts": {
    "positivo": 5,
    "negativo": 3,
    "neutral": 2
  }
}
```

---

### 6. Crear comentario con AI (Gemini)

- **POST** `/api/comments/ai`
- **Body** (`JSON`) igual que `/api/comments`.
- El `sentiment` se determina usando AI.
- **Response:** `201 Created` con el comentario creado.

---

### 7. Resumen AI de comentarios de un producto

- **GET** `/api/comments/{productId}/ai-summary`
- Genera un resumen breve de las reseñas de un producto usando AI.
- **Response:**
```json
{
  "productId": "PROD001",
  "summary": "La mayoría de los usuarios consideran que el producto es excelente y cumple con lo esperado, aunque algunos mencionan pequeños problemas de funcionamiento."
}
```

---

## Suposiciones y decisiones de diseño

- Sentimientos determinados por palabras clave (`positivo`, `negativo`, `neutral`).
- AI se usa solo en endpoints `/api/comments/ai` y `/ai-summary`.
- No se maneja autenticación para la API (desarrollo local).
- `CreatedAt` se genera automáticamente en UTC.
- Todas las propiedades de `Comment` son obligatorias.

