const ProductoAPI = (() => {
  const baseUrl = `${window.APP_CONFIG.apiUrl}/producto`;

  function catalogoLocal() {
    return Array.isArray(window.CATALOGO_LOCAL) ? window.CATALOGO_LOCAL : [];
  }

  function filtrarLocal({ busqueda, marca, categoria, combustible, orden } = {}) {
    let lista = catalogoLocal().slice();

    if (busqueda) {
      const termino = busqueda.toLowerCase();
      lista = lista.filter((p) =>
        [p.nombre, p.marca, p.categoria, p.descripcion]
          .join(' ')
          .toLowerCase()
          .includes(termino),
      );
    }
    if (marca) lista = lista.filter((p) => p.marca === marca);
    if (categoria) lista = lista.filter((p) => p.categoria === categoria);
    if (combustible) lista = lista.filter((p) => p.combustible === combustible);

    switch (orden) {
      case 'precio-asc':
        lista.sort((a, b) => a.precio - b.precio);
        break;
      case 'precio-desc':
        lista.sort((a, b) => b.precio - a.precio);
        break;
      case 'anio-desc':
        lista.sort((a, b) => b.anio - a.anio);
        break;
      case 'nombre-asc':
        lista.sort((a, b) => a.nombre.localeCompare(b.nombre, 'es'));
        break;
      default:
        lista.sort((a, b) => (b.destacado ? 1 : 0) - (a.destacado ? 1 : 0));
    }

    return lista;
  }

  async function request(url, options = {}) {
    const opciones = { credentials: 'include', ...options };
    let respuesta;
    try {
      respuesta = await fetch(url, opciones);
    } catch (error) {
      throw new Error('No se pudo conectar con la API. Verifica que el backend esté en ejecución.');
    }

    if (respuesta.status === 204) {
      return null;
    }

    const texto = await respuesta.text();
    const contenido = texto ? safeJson(texto) : null;

    if (!respuesta.ok) {
      if (respuesta.status === 401) {
        throw new Error('No autorizado. Inicia sesión para continuar.');
      }
      const mensaje = typeof contenido === 'string' ? contenido : contenido?.title || contenido?.detail;
      throw new Error(mensaje || `Error ${respuesta.status} al comunicarse con la API.`);
    }

    return contenido;
  }

  function safeJson(texto) {
    try {
      return JSON.parse(texto);
    } catch {
      return texto;
    }
  }

  function jsonOptions(method, body) {
    return {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    };
  }

  return {
    async listar(filtros = {}) {
      const { busqueda, marca, categoria, combustible, orden } = filtros;
      const params = new URLSearchParams();
      if (busqueda) params.set('busqueda', busqueda);
      if (marca) params.set('marca', marca);
      if (categoria) params.set('categoria', categoria);
      if (combustible) params.set('combustible', combustible);
      if (orden) params.set('orden', orden);

      const query = params.toString();
      try {
        const remoto = await request(query ? `${baseUrl}?${query}` : baseUrl);
        if (Array.isArray(remoto) && remoto.length) return remoto;
      } catch {
        console.warn('[AutoPrime] API no disponible. Mostrando catálogo local de demostración.');
      }
      return filtrarLocal(filtros);
    },

    async obtener(id) {
      try {
        const remoto = await request(`${baseUrl}/${id}`);
        if (remoto) return remoto;
      } catch {
        console.warn('[AutoPrime] API no disponible. Buscando en el catálogo local.');
      }
      return catalogoLocal().find((p) => p.id === Number(id)) || null;
    },

    async marcas() {
      try {
        const remoto = await request(`${baseUrl}/filtros/marcas`);
        if (Array.isArray(remoto) && remoto.length) return remoto;
      } catch {
        console.warn('[AutoPrime] API no disponible. Marcas desde el catálogo local.');
      }
      return [...new Set(catalogoLocal().map((p) => p.marca))].sort((a, b) => a.localeCompare(b, 'es'));
    },

    async categorias() {
      try {
        const remoto = await request(`${baseUrl}/filtros/categorias`);
        if (Array.isArray(remoto) && remoto.length) return remoto;
      } catch {
        console.warn('[AutoPrime] API no disponible. Categorías desde el catálogo local.');
      }
      return [...new Set(catalogoLocal().map((p) => p.categoria))].sort((a, b) => a.localeCompare(b, 'es'));
    },

    async combustibles() {
      try {
        const remoto = await request(`${baseUrl}/filtros/combustibles`);
        if (Array.isArray(remoto) && remoto.length) return remoto;
      } catch {
        // Fallback desde catálogo local
      }
      return [...new Set(catalogoLocal().map((p) => p.combustible).filter(Boolean))].sort((a, b) => a.localeCompare(b, 'es'));
    },

    crear(producto) {
      return request(baseUrl, jsonOptions('POST', producto));
    },

    actualizar(id, producto) {
      return request(`${baseUrl}/${id}`, jsonOptions('PUT', producto));
    },

    eliminar(id) {
      return request(`${baseUrl}/${id}`, { method: 'DELETE' });
    },

    subirImagen(id, archivo) {
      const data = new FormData();
      data.append('archivo', archivo);
      return request(`${baseUrl}/${id}/imagen`, { method: 'POST', body: data });
    },

    eliminarImagen(id) {
      return request(`${baseUrl}/${id}/imagen`, { method: 'DELETE' });
    },
  };
})();
