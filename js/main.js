document.addEventListener('DOMContentLoaded', () => {
  const grid = document.getElementById('grid-productos');
  const carga = document.getElementById('estado-carga');
  const sinResultados = document.getElementById('sin-resultados');
  const contador = document.getElementById('contador-resultados');
  const btnLimpiar = document.getElementById('btn-limpiar-filtros');

  const filtros = {
    busqueda: document.getElementById('filtro-busqueda'),
    marca: document.getElementById('filtro-marca'),
    categoria: document.getElementById('filtro-categoria'),
    combustible: document.getElementById('filtro-combustible'),
    orden: document.getElementById('filtro-orden'),
  };

  let debounceId;

  function valoresActuales() {
    return {
      busqueda: filtros.busqueda ? filtros.busqueda.value.trim() : '',
      marca: filtros.marca ? filtros.marca.value : '',
      categoria: filtros.categoria ? filtros.categoria.value : '',
      combustible: filtros.combustible ? filtros.combustible.value : '',
      orden: filtros.orden ? filtros.orden.value : '',
    };
  }

  function crearCard(producto, indice = 0) {
    const columna = document.createElement('div');
    columna.className = 'col-xl-4 col-md-6';
    columna.style.setProperty('--anim-delay', `${Math.min(indice, 12) * 60}ms`);

    const descuento = Number(producto.descuentoPorcentaje) || 0;
    const tieneOriginal = descuento > 0 && producto.precioOriginal;
    const destacado = producto.destacado ? '<span class="badge-destacado">Destacado</span>' : '';
    const agotadoOverlay = producto.agotado ? '<div class="agotado-overlay">Agotado</div>' : '';
    const stock = Number(producto.stock) > 0
      ? `${Utils.formatoNumero(producto.stock)} disponibles`
      : 'Agotado';

    const textoWhatsApp = `¡Hola AutoPrime! 🚗 Me interesa el vehículo *${producto.marca} ${producto.nombre}* (${producto.anio}) con precio de ${Utils.formatoCOP(producto.precio)}. ¿Sigue disponible?`;
    const urlWhatsApp = `https://wa.me/573001234567?text=${encodeURIComponent(textoWhatsApp)}`;

    columna.innerHTML = `
      <article class="card card-producto shadow-sm h-100">
        <div class="position-relative">
          <img src="${Utils.escape(Utils.imagenSegura(producto))}" class="card-img-top" alt="${Utils.escape(producto.nombre)}" loading="lazy">
          ${descuento > 0 ? `<span class="badge-descuento">-${descuento}%</span>` : ''}
          ${destacado}
          ${agotadoOverlay}
        </div>
        <div class="card-body d-flex flex-column">
          <div class="d-flex justify-content-between align-items-center mb-1">
            <span class="text-uppercase text-muted small fw-semibold">${Utils.escape(producto.marca)}</span>
            ${producto.combustible ? `<span class="badge bg-secondary-subtle text-dark" style="font-size: 0.7rem">${Utils.escape(producto.combustible)}</span>` : ''}
          </div>
          <h3 class="h6 fw-bold mb-2">${Utils.escape(producto.nombre)}</h3>
          
          <ul class="spec-lista mb-3">
            <li><span>Año</span><span>${Utils.escape(producto.anio)}</span></li>
            <li><span>Kilometraje</span><span>${producto.kilometraje > 0 ? `${Utils.formatoNumero(producto.kilometraje)} km` : '0 km (Nuevo)'}</span></li>
            <li><span>Transmisión</span><span>${Utils.escape(producto.transmision || 'Automática')}</span></li>
            <li><span>Disponibles</span><span>${stock}</span></li>
          </ul>

          <div class="mt-auto">
            <div class="d-flex align-items-baseline gap-2 mb-3">
              <span class="precio-actual">${Utils.formatoCOP(producto.precio)}</span>
              ${tieneOriginal ? `<span class="precio-original small">${Utils.formatoCOP(producto.precioOriginal)}</span>` : ''}
            </div>

            <!-- Acciones Automotrices -->
            <div class="d-flex flex-column gap-2">
              <div class="d-flex gap-2">
                <a class="btn btn-accent flex-grow-1" href="./producto.html?id=${producto.id}">Ver detalles</a>
                <a class="btn btn-whatsapp-outline px-3" href="${urlWhatsApp}" target="_blank" rel="noopener noreferrer" title="Consultar por WhatsApp" aria-label="Consultar por WhatsApp">
                  <svg viewBox="0 0 24 24" fill="currentColor" width="18" height="18">
                    <path d="M12 2a10 10 0 0 0-8.6 15l-1.3 4.7 4.8-1.3A10 10 0 1 0 12 2zm0 18a8 8 0 0 1-4.1-1.1l-.3-.2-2.8.8.8-2.8-.2-.3A8 8 0 1 1 12 20zm4.4-5.9c-.2-.1-1.4-.7-1.6-.8-.2-.1-.4-.1-.5.1l-.7.9c-.1.2-.3.2-.5.1a6.5 6.5 0 0 1-3.2-2.8c-.1-.2 0-.4.1-.5l.4-.5c.1-.2.1-.3 0-.5l-.7-1.7c-.2-.4-.4-.4-.5-.4h-.5c-.2 0-.5.1-.7.3-.2.2-.9.9-.9 2.1s.9 2.4 1 2.6c.1.2 1.8 2.7 4.3 3.8.6.3 1.1.4 1.5.5.6.2 1.2.2 1.6.1.5-.1 1.4-.6 1.6-1.1.2-.5.2-1 .1-1.1-.1-.1-.2-.2-.4-.3z"/>
                  </svg>
                </a>
              </div>
              <button class="btn btn-outline-accent w-100" data-agregar="${producto.id}" ${producto.agotado ? 'disabled' : ''}>
                Agregar al carrito
              </button>
            </div>
          </div>
        </div>
      </article>`;

    const boton = columna.querySelector('[data-agregar]');
    if (boton && !producto.agotado) {
      boton.addEventListener('click', () => {
        Cart.agregar(producto);
        const originalText = boton.innerHTML;
        boton.innerHTML = '✓ ¡Agregado!';
        boton.classList.replace('btn-outline-accent', 'btn-success');
        Utils.mostrarToast(`¡${producto.nombre} agregado al carrito!`, 'success');

        setTimeout(() => {
          boton.innerHTML = originalText;
          boton.classList.replace('btn-success', 'btn-outline-accent');
        }, 1500);
      });
    }

    return columna;
  }

  function render(productos) {
    grid.innerHTML = '';
    contador.textContent = `${productos.length} vehículo(s) disponible(s)`;

    if (!productos.length) {
      grid.classList.add('d-none');
      sinResultados.classList.remove('d-none');
      return;
    }

    sinResultados.classList.add('d-none');
    grid.classList.remove('d-none');
    const fragmento = document.createDocumentFragment();
    productos.forEach((producto, indice) => fragmento.appendChild(crearCard(producto, indice)));
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
      const [marcas, categorias, combustibles] = await Promise.all([
        ProductoAPI.marcas(),
        ProductoAPI.categorias(),
        ProductoAPI.combustibles(),
      ]);

      if (filtros.marca) {
        marcas.forEach((marca) => {
          filtros.marca.appendChild(new Option(marca, marca));
        });
      }

      if (filtros.categoria) {
        categorias.forEach((categoria) => {
          filtros.categoria.appendChild(new Option(categoria, categoria));
        });
      }

      if (filtros.combustible) {
        combustibles.forEach((combustible) => {
          filtros.combustible.appendChild(new Option(combustible, combustible));
        });
      }
    } catch {
      Utils.mostrarToast('No se pudieron cargar los filtros.', 'warning');
    }
  }

  async function cargarBannerDestacado() {
    const nombres = {
      imagen: document.getElementById('banner-imagen'),
      titulo: document.getElementById('banner-nombre'),
      desc: document.getElementById('banner-desc'),
      precio: document.getElementById('banner-precio'),
      cta: document.getElementById('banner-cta'),
    };
    if (!nombres.titulo) return;

    try {
      const productos = await ProductoAPI.listar({});
      const destacados = productos.filter((p) => p.destacado);
      const elegido = (destacados.length ? destacados : productos)
        .slice()
        .sort((a, b) => Number(b.precio) - Number(a.precio))[0];
      if (!elegido) return;

      if (nombres.imagen && elegido.imagenUrl) {
        nombres.imagen.src = elegido.imagenUrl;
        nombres.imagen.alt = `${elegido.marca} ${elegido.nombre}`;
      }
      nombres.titulo.textContent = elegido.nombre;
      if (nombres.desc) {
        nombres.desc.textContent = elegido.descripcion ||
          `Vehículo ${elegido.marca} ${elegido.anio} con entrega inmediata.`;
      }
      if (nombres.precio) {
        const original =
          Number(elegido.precioOriginal) > Number(elegido.precio)
            ? `<small>${Utils.formatoCOP(elegido.precioOriginal)}</small>`
            : '';
        nombres.precio.innerHTML = `${Utils.formatoCOP(elegido.precio)}${original}`;
      }
      if (nombres.cta) {
        nombres.cta.href = `./producto.html?id=${elegido.id}`;
      }
    } catch {
      // El banner conserva su contenido por defecto.
    }
  }

  const botonBuscar = document.querySelector('[data-buscar]');
  if (botonBuscar) {
    botonBuscar.addEventListener('click', () => {
      if (filtros.busqueda) {
        filtros.busqueda.scrollIntoView({ behavior: 'smooth', block: 'center' });
        filtros.busqueda.focus();
      }
    });
  }

  if (btnLimpiar) {
    btnLimpiar.addEventListener('click', () => {
      if (filtros.busqueda) filtros.busqueda.value = '';
      if (filtros.marca) filtros.marca.value = '';
      if (filtros.categoria) filtros.categoria.value = '';
      if (filtros.combustible) filtros.combustible.value = '';
      if (filtros.orden) filtros.orden.value = '';
      cargarProductos();
    });
  }

  async function cargarMenuCategorias() {
    const menu = document.getElementById('menu-categorias');
    if (!menu) return;

    const crearItem = (texto, categoria) => {
      const item = document.createElement('li');
      const enlace = document.createElement('a');
      enlace.className = 'dropdown-item';
      enlace.href = '#catalogo';
      enlace.textContent = texto;
      enlace.addEventListener('click', () => {
        if (filtros.categoria) filtros.categoria.value = categoria;
        cargarProductos();
      });
      item.appendChild(enlace);
      return item;
    };

    try {
      const categorias = await ProductoAPI.categorias();
      menu.innerHTML = '';
      menu.appendChild(crearItem('Todas las categorías', ''));
      categorias.forEach((categoria) => {
        menu.appendChild(crearItem(categoria, categoria));
      });
    } catch {
      // Sin categorías dinámicas si la API no responde.
    }
  }

  Object.values(filtros).forEach((control) => {
    if (!control) return;
    const evento = control === filtros.busqueda ? 'input' : 'change';
    control.addEventListener(evento, () => {
      clearTimeout(debounceId);
      debounceId = setTimeout(cargarProductos, 300);
    });
  });

  cargarFiltros();
  cargarMenuCategorias();
  cargarBannerDestacado();
  cargarProductos();
});
