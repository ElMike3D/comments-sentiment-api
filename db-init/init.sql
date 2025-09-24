-- Crear la base de datos
CREATE DATABASE SentimentDb;
GO
USE SentimentDb;
GO

-- Crear la tabla Comments
CREATE TABLE Comments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId NVARCHAR(50) NOT NULL,
    UserId NVARCHAR(50) NOT NULL,
    CommentText NVARCHAR(MAX) NOT NULL,
    Sentiment NVARCHAR(20) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- Crear login y usuario
CREATE LOGIN test_admin WITH PASSWORD = 'Password123';
CREATE USER test_admin FOR LOGIN test_admin;
ALTER ROLE db_owner ADD MEMBER test_admin;
GO

-- Insertar datos de prueba
INSERT INTO Comments (ProductId, UserId, CommentText, Sentiment) VALUES
('PROD001', 'USER001', 'Este producto es excelente, superó mis expectativas.', 'positivo'),
('PROD001', 'USER002', 'Muy bueno y funcional, me encanta.', 'positivo'),
('PROD001', 'USER003', 'Funciona pero tiene un defecto menor.', 'neutral'),
('PROD002', 'USER004', 'Terrible experiencia, vino con un problema.', 'negativo'),
('PROD002', 'USER005', 'No está mal, pero esperaba más.', 'neutral'),
('PROD003', 'USER006', 'Increíble calidad y buen diseño.', 'positivo'),
('PROD003', 'USER007', 'Malo, llegó roto y con defectos.', 'negativo'),
('PROD004', 'USER008', 'Fantástico, superó lo esperado.', 'positivo'),
('PROD004', 'USER009', 'Horrible, no funciona.', 'negativo');
GO
