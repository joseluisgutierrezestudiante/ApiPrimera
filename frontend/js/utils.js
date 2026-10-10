const Utils = {
  formatoCOP(valor) {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      maximumFractionDigits: 0,
    }).format(Number(valor) || 0);
  },

  formatoNumero(valor) {
    return new Intl.NumberFormat('es-CO').format(Number(valor) || 0);
  },

  imagenSegura(producto) {
    return producto && producto.imagenUrl
      ? producto.imagenUrl
      : 'https://placehold.co/640x420/e9ecef/6c757d?text=Sin+imagen';
  },

  escape(texto) {
    const div = document.createElement('div');
    div.textContent = texto == null ? '' : String(texto);
    return div.innerHTML;
  },

  parametroUrl(nombre) {
    return new URLSearchParams(window.location.search).get(nombre);
  },

  mostrarToast(mensaje, tipo = 'success') {
    let contenedor = document.getElementById('toast-contenedor');
    if (!contenedor) {
      contenedor = document.createElement('div');
      contenedor.id = 'toast-contenedor';
      contenedor.className = 'toast-container position-fixed bottom-0 end-0 p-3';
      document.body.appendChild(contenedor);
    }

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-bg-${tipo} border-0`;
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
      <div class="d-flex">
        <div class="toast-body">${Utils.escape(mensaje)}</div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
      </div>`;

    contenedor.appendChild(toast);
    const instancia = bootstrap.Toast.getOrCreateInstance(toast, { delay: 3000 });
    instancia.show();
    toast.addEventListener('hidden.bs.toast', () => toast.remove());
  },
};
