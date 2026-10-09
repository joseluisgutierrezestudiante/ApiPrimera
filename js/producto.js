document.addEventListener('DOMContentLoaded', async () => {
  const carga = document.getElementById('estado-carga');
  const contenedor = document.getElementById('detalle-producto');
  const id = Number(Utils.parametroUrl('id'));

  if (!id) {
    carga.innerHTML = '<div class="alert alert-warning">No se indicó un vehículo válido.</div>';
    return;
  }

  try {
    const producto = await ProductoAPI.obtener(id);
    contenedor.innerHTML = plantilla(producto);
    contenedor.classList.remove('d-none');

    const input = document.getElementById('cantidad');
    const botonAgregar = document.getElementById('btn-agregar');

    botonAgregar.addEventListener('click', () => {
      const cantidad = Math.max(1, Number(input.value) || 1);
      Cart.agregar(producto, cantidad);
      Utils.mostrarToast(`${producto.nombre} agregado al carrito.`);
    });
  } catch (error) {
    carga.innerHTML = `<div class="alert alert-danger"><strong>Error:</strong> ${Utils.escape(error.message)}</div>`;
    return;
  }

  carga.classList.add('d-none');
});

function plantilla(producto) {
  const descuento = Number(producto.descuentoPorcentaje) || 0;
  const tieneOriginal = descuento > 0 && producto.precioOriginal;
  const agotado = producto.agotado;

  return `
    <div class="row g-5">
      <div class="col-lg-6">
        <div class="position-relative rounded-4 overflow-hidden shadow-sm">
          <img src="${Utils.escape(Utils.imagenSegura(producto))}" class="img-fluid w-100" alt="${Utils.escape(producto.nombre)}">
          ${descuento > 0 ? `<span class="badge-descuento">-${descuento}%</span>` : ''}
          ${agotado ? '<div class="agotado-overlay">Agotado</div>' : ''}
        </div>
      </div>
      <div class="col-lg-6">
        <div class="text-uppercase text-muted fw-semibold">${Utils.escape(producto.marca)} · ${Utils.escape(producto.categoria)}</div>
        <h1 class="h3 fw-bold">${Utils.escape(producto.nombre)}</h1>
        <div class="d-flex align-items-baseline gap-3 my-3">
          <span class="precio-actual fs-3">${Utils.formatoCOP(producto.precio)}</span>
          ${tieneOriginal ? `<span class="precio-original">${Utils.formatoCOP(producto.precioOriginal)}</span>` : ''}
        </div>
        <p class="text-muted">${Utils.escape(producto.descripcion)}</p>

        <div class="row row-cols-2 g-3 my-4">
          ${dato('Año', producto.anio)}
          ${dato('Kilometraje', `${Utils.formatoNumero(producto.kilometraje)} km`)}
          ${dato('Combustible', producto.combustible)}
          ${dato('Transmisión', producto.transmision)}
          ${dato('Color', producto.color)}
          ${dato('Stock', agotado ? 'Sin unidades' : `${producto.stock} unidades`)}
        </div>

        <div class="d-flex gap-2 align-items-center">
          <input id="cantidad" type="number" class="form-control" value="1" min="1" style="max-width: 5rem">
          <button id="btn-agregar" class="btn btn-accent btn-lg flex-grow-1" ${agotado ? 'disabled' : ''}>
            ${agotado ? 'No disponible' : 'Agregar al carrito'}
          </button>
        </div>
      </div>
    </div>`;
}

function dato(etiqueta, valor) {
  return `
    <div class="col">
      <div class="border rounded-3 p-3 h-100 superficie">
        <div class="text-muted small">${Utils.escape(etiqueta)}</div>
        <div class="fw-semibold">${Utils.escape(valor)}</div>
      </div>
    </div>`;
}
