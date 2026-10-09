const ProductoAPI = (() => {
  const baseUrl = `${window.APP_CONFIG.apiUrl}/producto`;

  async function request(url, options = {}) {
    let respuesta;
    try {
      respuesta = await fetch(url, options);
    } catch (error) {
      throw new Error('No se pudo conectar con la API. Verifica que el backend esté en ejecución.');
    }

    if (respuesta.status === 204) {
      return null;
    }

    const texto = await respuesta.text();
    const contenido = texto ? safeJson(texto) : null;

    if (!respuesta.ok) {
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
    async listar({ busqueda, marca, categoria, orden } = {}) {
      const params = new URLSearchParams();
      if (busqueda) params.set('busqueda', busqueda);
      if (marca) params.set('marca', marca);
      if (categoria) params.set('categoria', categoria);
      if (orden) params.set('orden', orden);

      const query = params.toString();
      return request(query ? `${baseUrl}?${query}` : baseUrl);
    },

    obtener(id) {
      return request(`${baseUrl}/${id}`);
    },

    marcas() {
      return request(`${baseUrl}/filtros/marcas`);
    },

    categorias() {
      return request(`${baseUrl}/filtros/categorias`);
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
