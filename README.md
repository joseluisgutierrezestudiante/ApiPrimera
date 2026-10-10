# ApiPrimera Store

Proyecto de taller: una API REST de productos en ASP.NET Core (EF Core + MySQL), con fotos en
Cloudinary y un frontend que la consume. El catálogo son carros nuevos, a precio en pesos.

Autor: Joseluis Gutiérrez Machado.

## Cómo correrlo

Necesitas el SDK de .NET 10 y MySQL 8 en el puerto 3306.

Las credenciales no van en el repo; se guardan con **user-secrets**:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=EcommerceDB;User=root;Password=TU_CLAVE;"
dotnet run
```

Al arrancar se aplican las migraciones y, si la tabla `Producto` está vacía, se llena sola con el
catálogo de ejemplo. Si además quieres guardar las fotos en tu Cloudinary:

```bash
dotnet user-secrets set "CloudinarySettings:CloudName" "TU_CLOUD_NAME"
dotnet user-secrets set "CloudinarySettings:ApiKey" "TU_API_KEY"
dotnet user-secrets set "CloudinarySettings:ApiSecret" "TU_API_SECRET"
```

Cloudinary es opcional: sin configurarlo, la app guarda la URL de origen de cada foto.

## Páginas

| Ruta | Qué es |
| --- | --- |
| `/` | Catálogo (redirige a `/Home`) |
| `/Producto/{id}` | Detalle del carro |
| `/Carrito` | Carrito y checkout |
| `/Login`, `/Registro` | Entrar o crear cuenta |
| `/Admin/Productos` | Listar, editar y borrar (pide sesión) |
| `/Admin/Producto` | Alta con subida de imagen |
| `/openapi/v1.json` | Documentación OpenAPI |

## API

| Método | Ruta | Qué hace |
| --- | --- | --- |
| GET | `/api/producto` | Lista con filtros `busqueda`, `marca`, `categoria`, `orden` |
| GET | `/api/producto/filtros/marcas` | Marcas disponibles |
| GET | `/api/producto/filtros/categorias` | Categorías disponibles |
| GET | `/api/producto/{id}` | Detalle |
| POST | `/api/producto` | Crear |
| PUT | `/api/producto/{id}` | Editar (no toca la imagen) |
| DELETE | `/api/producto/{id}` | Borrar |
| POST | `/api/producto/{id}/imagen` | Subir imagen (`multipart` o `imagenBase64`) |
| DELETE | `/api/producto/{id}/imagen` | Quitar imagen |
| POST | `/api/auth/registro` | Crear cuenta |
| POST | `/api/auth/login` | Iniciar sesión |
| GET | `/api/auth/sesion` | Ver quién está logueado |
| POST | `/api/auth/logout` | Cerrar sesión |

Los endpoints que escriben (crear, editar, borrar, imágenes) piden sesión. La imagen acepta JPG,
PNG, WEBP, GIF y AVIF de hasta 5 MB; otro formato responde 415, no 500. Solo el endpoint de imagen
escribe `ImagenUrl`, así que un `PUT` normal no puede dejar fotos huérfanas, y al reemplazar una
imagen se borra la anterior.

## Pruebas

```bash
dotnet test
```

Son 79 pruebas. Cubren el repositorio (CRUD, filtros, orden), la autenticación, el carrito, el
validador de imágenes y el cliente HTTP contra un servidor levantado en el propio proceso. Las del
repositorio corren sobre SQLite en memoria, así que no necesitan MySQL.

## Estructura

```
Controllers/      API REST (Producto, Marca, Carro, Auth)
Repository/       Acceso a datos (EF Core)
Services/         Cloudinary, cliente HTTP, carrito, seed
Models/           Entidades, DTOs y ViewModels
Pages/            Razor Pages (catálogo, detalle, carrito, admin, login)
frontend/         La misma web en HTML/Bootstrap para servir aparte
Migrations/       Migraciones de EF Core
ApiPrimera.Tests  Pruebas
```

## Notas

- .NET 10 con EF Core 9.0.11 y Pomelo 9.0.0.
- El checkout es de práctica: simula el pago, no cobra de verdad.
- `frontend/` es la versión estática del sitio (mismos archivos que usas con Live Server); apunta a
  la API en `http://localhost:5069/api`.
