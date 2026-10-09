document.addEventListener('DOMContentLoaded', () => {
  const vacio = document.getElementById('carrito-vacio');
  const contenido = document.getElementById('carrito-contenido');
  const filas = document.getElementById('carrito-filas');
  const resumenUnidades = document.getElementById('resumen-unidades');
  const resumenTotal = document.getElementById('resumen-total');

  function render() {
    const items = Cart.items();

    if (!items.length) {
      vacio.classList.remove('d-none');
      contenido.classList.add('d-none');
      return;
    }

    vacio.classList.add('d-none');
    contenido.classList.remove('d-none');

    filas.innerHTML = items.map((item) => `
      <tr>
        <td>
          <div class="d-flex align-items-center gap-3">
            <img src="${Utils.escape(Utils.imagenSegura(item))}" alt="" width="72" height="52" class="rounded object-fit-cover">
            <div>
              <div class="fw-semibold">${Utils.escape(item.nombre)}</div>
              <div class="text-muted small">${Utils.escape(item.marca)} · ${Utils.formatoCOP(item.precio)}</div>
            </div>
          </div>
        </td>
        <td class="text-center" style="width: 120px">
          <input type="number" min="1" value="${item.cantidad}" class="form-control form-control-sm text-center"
                 data-cantidad="${item.id}">
        </td>
        <td class="text-end fw-semibold">${Utils.formatoCOP(item.precio * item.cantidad)}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-outline-danger" data-eliminar="${item.id}">Quitar</button>
        </td>
      </tr>`).join('');

    resumenUnidades.textContent = Cart.cantidadTotal();
    resumenTotal.textContent = Utils.formatoCOP(Cart.total());
  }

  filas.addEventListener('change', (evento) => {
    const id = Number(evento.target.dataset.cantidad);
    if (!id) return;
    Cart.cambiarCantidad(id, evento.target.value);
  });

  filas.addEventListener('click', (evento) => {
    const id = Number(evento.target.dataset.eliminar);
    if (!id) return;
    Cart.eliminar(id);
    Utils.mostrarToast('Vehículo eliminado del carrito.', 'warning');
  });

  document.getElementById('btn-vaciar').addEventListener('click', () => {
    if (confirm('¿Vaciar todo el carrito?')) {
      Cart.vaciar();
    }
  });

  document.getElementById('btn-finalizar').addEventListener('click', () => {
    if (!Cart.items().length) return;
    Cart.vaciar();
    Utils.mostrarToast('¡Compra registrada! Un asesor te contactará pronto.');
  });

  document.addEventListener('carrito:actualizado', render);
  render();
});
