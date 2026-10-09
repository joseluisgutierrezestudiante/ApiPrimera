const Cart = (() => {
  const CLAVE = 'autoprime_carrito';

  function leer() {
    try {
      const datos = JSON.parse(localStorage.getItem(CLAVE));
      return Array.isArray(datos) ? datos : [];
    } catch {
      return [];
    }
  }

  function guardar(items) {
    localStorage.setItem(CLAVE, JSON.stringify(items));
    actualizarBadge();
    document.dispatchEvent(new CustomEvent('carrito:actualizado', { detail: items }));
  }

  function indexPorId(items, id) {
    return items.findIndex((item) => item.id === id);
  }

  function agregar(producto, cantidad = 1) {
    const items = leer();
    const indice = indexPorId(items, producto.id);

    if (indice >= 0) {
      items[indice].cantidad += cantidad;
    } else {
      items.push({
        id: producto.id,
        nombre: producto.nombre,
        marca: producto.marca,
        precio: producto.precio,
        imagenUrl: producto.imagenUrl,
        stock: producto.stock,
        cantidad,
      });
    }

    guardar(items);
  }

  function eliminar(id) {
    guardar(leer().filter((item) => item.id !== id));
  }

  function cambiarCantidad(id, cantidad) {
    const items = leer();
    const indice = indexPorId(items, id);
    if (indice < 0) return;

    const nueva = Math.max(1, Number(cantidad) || 1);
    items[indice].cantidad = nueva;
    guardar(items);
  }

  function vaciar() {
    guardar([]);
  }

  function items() {
    return leer();
  }

  function cantidadTotal() {
    return leer().reduce((total, item) => total + item.cantidad, 0);
  }

  function total() {
    return leer().reduce((suma, item) => suma + item.precio * item.cantidad, 0);
  }

  function actualizarBadge() {
    const badge = document.getElementById('carrito-badge');
    if (!badge) return;

    const cantidad = cantidadTotal();
    badge.textContent = cantidad;
    badge.classList.toggle('d-none', cantidad === 0);
  }

  document.addEventListener('DOMContentLoaded', actualizarBadge);

  return { agregar, eliminar, cambiarCantidad, vaciar, items, cantidadTotal, total, actualizarBadge };
})();
