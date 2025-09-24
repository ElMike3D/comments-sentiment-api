# Comments Sentiment API

API para gestionar comentarios de productos y analizar su sentimiento de manera automática.

---

## 1) Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO>
cd comments-sentiment-api/SentimentApi
```

---

## 2) Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server (local o remoto) accesible desde tu equipo

---

## 3) Inicializar la base de datos

- Antes de ejecutar la API, crea la base y tablas ejecutando el script `db-init/init.sql` en tu servidor de SQL Server.

  - Con SQL Server Management Studio (SSMS): abre `db-init/init.sql` y ejecútalo.
  - Con `sqlcmd` (Windows):

```bash
# Desde la raíz del repositorio
sqlcmd -S localhost -d master -i "db-init\init.sql" -E
```

- La base creada es `SentimentDb` e incluye la tabla `Comments`.

---

## 4) Configurar conexión y API key

Edita `appsettings.json` en `SentimentApi/`:

- `ConnectionStrings:Default`: cadena de conexión de SQL Server.
- `Gemini:ApiKey`: reemplaza el valor por tu clave de API de Gemini.

La aplicación no usará `.env`; la clave debe estar definida en `appsettings.json`.

---

## 5) Construir y ejecutar la aplicación

1. Restaurar paquetes:

```bash
dotnet restore
```

2. Ejecutar la aplicación:

```bash
dotnet run
```

Por defecto, la API correrá en `https://localhost:7015` y `http://localhost:5019`. Documentación en `/swagger`.

---

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

