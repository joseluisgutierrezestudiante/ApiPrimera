document.addEventListener('DOMContentLoaded', () => {
  const grid = document.getElementById('grid-productos');
  const carga = document.getElementById('estado-carga');
  const sinResultados = document.getElementById('sin-resultados');
  const contador = document.getElementById('contador-resultados');

  const filtros = {
    busqueda: document.getElementById('filtro-busqueda'),
    marca: document.getElementById('filtro-marca'),
    categoria: document.getElementById('filtro-categoria'),
    orden: document.getElementById('filtro-orden'),
  };

  let debounceId;

  function valoresActuales() {
    return {
      busqueda: filtros.busqueda.value.trim(),
      marca: filtros.marca.value,
      categoria: filtros.categoria.value,
      orden: filtros.orden.value,
    };
  }

  function crearCard(producto) {
    const columna = document.createElement('div');
    columna.className = 'col-xl-4 col-md-6';

    const descuento = Number(producto.descuentoPorcentaje) || 0;
    const tieneOriginal = descuento > 0 && producto.precioOriginal;
    const destacado = producto.destacado ? '<span class="badge-destacado">Destacado</span>' : '';
    const agotadoOverlay = producto.agotado ? '<div class="agotado-overlay">Agotado</div>' : '';

    columna.innerHTML = `
      <article class="card card-producto shadow-sm">
        <div class="position-relative">
          <img src="${Utils.escape(Utils.imagenSegura(producto))}" class="card-img-top" alt="${Utils.escape(producto.nombre)}">
          ${descuento > 0 ? `<span class="badge-descuento">-${descuento}%</span>` : ''}
          ${destacado}
          ${agotadoOverlay}
        </div>
        <div class="card-body d-flex flex-column">
          <div class="text-uppercase text-muted small fw-semibold">${Utils.escape(producto.marca)}</div>
          <h3 class="h6 fw-bold">${Utils.escape(producto.nombre)}</h3>
          <ul class="spec-lista mb-3">
            <li><span>Año</span><span>${Utils.escape(producto.anio)}</span></li>
            <li><span>Kilometraje</span><span>${Utils.formatoNumero(producto.kilometraje)} km</span></li>
            <li><span>Transmisión</span><span>${Utils.escape(producto.transmision)}</span></li>
            <li><span>Combustible</span><span>${Utils.escape(producto.combustible)}</span></li>
          </ul>
          <div class="mt-auto">
            <div class="d-flex align-items-baseline gap-2 mb-3">
              <span class="precio-actual">${Utils.formatoCOP(producto.precio)}</span>
              ${tieneOriginal ? `<span class="precio-original small">${Utils.formatoCOP(producto.precioOriginal)}</span>` : ''}
            </div>
            <div class="d-flex gap-2">
              <a class="btn btn-outline-accent flex-grow-1" href="./producto.html?id=${producto.id}">Ver detalle</a>
              <button class="btn btn-accent flex-grow-1" data-agregar="${producto.id}" ${producto.agotado ? 'disabled' : ''}>
                Agregar
              </button>
            </div>
          </div>
        </div>
      </article>`;

    const boton = columna.querySelector('[data-agregar]');
    if (boton && !producto.agotado) {
      boton.addEventListener('click', () => {
        Cart.agregar(producto);
        Utils.mostrarToast(`${producto.nombre} agregado al carrito.`);
      });
    }

    return columna;
  }

  function render(productos) {
    grid.innerHTML = '';
    contador.textContent = `${productos.length} vehículo(s)`;

    if (!productos.length) {
      grid.classList.add('d-none');
      sinResultados.classList.remove('d-none');
      return;
    }

    sinResultados.classList.add('d-none');
    grid.classList.remove('d-none');
    const fragmento = document.createDocumentFragment();
    productos.forEach((producto) => fragmento.appendChild(crearCard(producto)));
    grid.appendChild(fragmento);
  }

  async function cargarProductos() {
    carga.classList.remove('d-none');
    grid.classList.add('d-none');
    sinResultados.classList.add('d-none');

    try {
      const productos = await ProductoAPI.listar(valoresActuales());
      render(productos);
    } catch (error) {
      loaderError(error.message);
    } finally {
      carga.classList.add('d-none');
    }
  }

  function loaderError(mensaje) {
    grid.innerHTML = '';
    grid.classList.add('d-none');
    sinResultados.classList.remove('d-none');
    sinResultados.innerHTML = `
      <div class="alert alert-danger d-inline-block">
        <strong>Error:</strong> ${Utils.escape(mensaje)}
      </div>`;
  }

  async function cargarFiltros() {
    try {
      const [marcas, categorias] = await Promise.all([ProductoAPI.marcas(), ProductoAPI.categorias()]);
      marcas.forEach((marca) => {
        filtros.marca.appendChild(new Option(marca, marca));
      });
      categorias.forEach((categoria) => {
        filtros.categoria.appendChild(new Option(categoria, categoria));
      });
    } catch {
      Utils.mostrarToast('No se pudieron cargar los filtros.', 'warning');
    }
  }

  Object.values(filtros).forEach((control) => {
    const evento = control === filtros.busqueda ? 'input' : 'change';
    control.addEventListener(evento, () => {
      clearTimeout(debounceId);
      debounceId = setTimeout(cargarProductos, 300);
    });
  });

  cargarFiltros();
  cargarProductos();
});
