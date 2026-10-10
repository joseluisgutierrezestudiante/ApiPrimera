document.addEventListener('DOMContentLoaded', () => {
  const vacio = document.getElementById('carrito-vacio');
  const contenido = document.getElementById('carrito-contenido');
  const filas = document.getElementById('carrito-filas');
  const resumenUnidades = document.getElementById('resumen-unidades');
  const resumenTotal = document.getElementById('resumen-total');

  const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const VENC_REGEX = /^(0[1-9]|1[0-2])\/\d{2}$/;
  const ENVIO = 0;

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
          <div class="fw-semibold">${Utils.escape(item.nombre)}</div>
          <div class="text-muted small">${Utils.escape(item.marca)} · ${Utils.formatoCOP(item.precio)}</div>
        </td>
        <td class="text-center" style="width: 120px">
          <input type="number" class="form-control form-control-sm text-center" value="${item.cantidad}" min="1"
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

  // ---------- Checkout simulado por pasos ----------

  const modalEl = document.getElementById('modalCheckout');
  const modal = new bootstrap.Modal(modalEl);
  const btnVolver = document.getElementById('co-volver');
  const btnCancelar = document.getElementById('co-cancelar');
  const btnSiguiente = document.getElementById('co-siguiente');

  const campos = {
    nombre: document.getElementById('co-nombre'),
    email: document.getElementById('co-email'),
    telefono: document.getElementById('co-telefono'),
    ciudad: document.getElementById('co-ciudad'),
    direccion: document.getElementById('co-direccion'),
    tarjeta: document.getElementById('co-tarjeta'),
    titular: document.getElementById('co-titular'),
    venc: document.getElementById('co-venc'),
    cvv: document.getElementById('co-cvv'),
    banco: document.getElementById('co-banco'),
  };

  const paneles = {
    tarjeta: document.getElementById('co-panel-tarjeta'),
    pse: document.getElementById('co-panel-pse'),
    contraentrega: document.getElementById('co-panel-contra'),
  };

  let pasoActual = 1;
  let procesando = false;

  function marcar(input, valido) {
    input.classList.toggle('is-invalid', !valido);
    return valido;
  }

  function metodoPago() {
    return modalEl.querySelector('input[name="co-metodo"]:checked').value;
  }

  function actualizarIndicadores(paso) {
    document.querySelectorAll('[data-indicador]').forEach((li) => {
      const n = Number(li.dataset.indicador);
      if (paso === 'ok') {
        li.classList.add('is-done');
        li.classList.remove('is-active');
      } else {
        li.classList.toggle('is-active', n === paso);
        li.classList.toggle('is-done', n < paso);
      }
    });
  }

  function irA(paso) {
    pasoActual = paso;
    document.querySelectorAll('[data-paso]').forEach((seccion) => {
      seccion.classList.toggle('d-none', seccion.dataset.paso !== String(paso));
    });
    actualizarIndicadores(paso);

    const esOk = paso === 'ok';
    btnVolver.classList.toggle('d-none', esOk || paso === 1);
    btnCancelar.classList.toggle('d-none', esOk);
    btnSiguiente.classList.toggle('d-none', esOk);
    btnSiguiente.textContent = paso === 3 ? 'Pagar y confirmar' : 'Continuar';
  }

  function seleccionarMetodo() {
    const metodo = metodoPago();
    Object.entries(paneles).forEach(([clave, panel]) => {
      panel.classList.toggle('d-none', clave !== metodo);
    });
    modalEl.querySelectorAll('.checkout-metodo').forEach((label) => {
      label.classList.toggle('is-selected', label.querySelector('input').value === metodo);
    });
  }

  modalEl.querySelectorAll('input[name="co-metodo"]').forEach((radio) => {
    radio.addEventListener('change', seleccionarMetodo);
  });

  campos.tarjeta.addEventListener('input', () => {
    const digitos = campos.tarjeta.value.replace(/\D/g, '').slice(0, 16);
    campos.tarjeta.value = digitos.replace(/(.{4})/g, '$1 ').trim();
  });

  campos.venc.addEventListener('input', () => {
    const digitos = campos.venc.value.replace(/\D/g, '').slice(0, 4);
    campos.venc.value = digitos.length > 2 ? `${digitos.slice(0, 2)}/${digitos.slice(2)}` : digitos;
  });

  campos.cvv.addEventListener('input', () => {
    campos.cvv.value = campos.cvv.value.replace(/\D/g, '').slice(0, 4);
  });

  function validarEntrega() {
    let valido = true;
    valido = marcar(campos.nombre, campos.nombre.value.trim().length >= 3) && valido;
    valido = marcar(campos.email, EMAIL_REGEX.test(campos.email.value.trim())) && valido;
    valido = marcar(campos.telefono, campos.telefono.value.replace(/\D/g, '').length >= 7) && valido;
    valido = marcar(campos.ciudad, campos.ciudad.value.trim().length >= 2) && valido;
    valido = marcar(campos.direccion, campos.direccion.value.trim().length >= 5) && valido;
    return valido;
  }

  function validarPago() {
    const metodo = metodoPago();

    if (metodo === 'tarjeta') {
      let valido = true;
      valido = marcar(campos.tarjeta, campos.tarjeta.value.replace(/\D/g, '').length === 16) && valido;
      valido = marcar(campos.titular, campos.titular.value.trim().length >= 3) && valido;
      valido = marcar(campos.venc, VENC_REGEX.test(campos.venc.value.trim())) && valido;
      valido = marcar(campos.cvv, /^\d{3,4}$/.test(campos.cvv.value)) && valido;
      return valido;
    }

    if (metodo === 'pse') {
      return marcar(campos.banco, campos.banco.value !== '');
    }

    return true;
  }

  function descripcionPago() {
    const metodo = metodoPago();
    if (metodo === 'tarjeta') {
      const ultimos = campos.tarjeta.value.replace(/\D/g, '').slice(-4);
      return `Tarjeta •••• ${ultimos}`;
    }
    if (metodo === 'pse') {
      return `PSE · ${campos.banco.value}`;
    }
    return 'Pago contra entrega';
  }

  function renderResumen() {
    const items = Cart.items();
    document.getElementById('co-resumen-items').innerHTML = items.map((item) => `
      <div class="d-flex justify-content-between py-1">
        <span>${Utils.escape(item.nombre)} <span class="text-muted">x${item.cantidad}</span></span>
        <span class="fw-semibold">${Utils.formatoCOP(item.precio * item.cantidad)}</span>
      </div>`).join('');

    const subtotal = Cart.total();
    document.getElementById('co-subtotal').textContent = Utils.formatoCOP(subtotal);
    document.getElementById('co-envio').textContent = ENVIO > 0 ? Utils.formatoCOP(ENVIO) : 'Gratis';
    document.getElementById('co-total').textContent = Utils.formatoCOP(subtotal + ENVIO);

    document.getElementById('co-resumen-entrega').innerHTML =
      `${Utils.escape(campos.nombre.value.trim())}<br>${Utils.escape(campos.direccion.value.trim())}, ${Utils.escape(campos.ciudad.value.trim())}<br>${Utils.escape(campos.telefono.value.trim())} · ${Utils.escape(campos.email.value.trim())}`;
    document.getElementById('co-resumen-pago').textContent = descripcionPago();
  }

  function generarOrden() {
    const base = Date.now().toString(36).toUpperCase().slice(-6);
    const azar = Math.floor(Math.random() * 900 + 100);
    return `AP-${base}-${azar}`;
  }

  async function confirmarPago() {
    if (procesando) return;
    procesando = true;
    btnSiguiente.disabled = true;
    btnSiguiente.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Procesando pago...';
    btnVolver.disabled = true;

    await new Promise((resolver) => setTimeout(resolver, 1400));

    document.getElementById('co-orden').textContent = generarOrden();
    irA('ok');
    Cart.vaciar();
    Utils.mostrarToast('¡Pago simulado exitoso! Compra finalizada.', 'success');

    procesando = false;
    btnSiguiente.disabled = false;
    btnVolver.disabled = false;
  }

  btnSiguiente.addEventListener('click', () => {
    if (pasoActual === 1) {
      if (!validarEntrega()) {
        Utils.mostrarToast('Completa tus datos de entrega.', 'warning');
        return;
      }
      irA(2);
    } else if (pasoActual === 2) {
      if (!validarPago()) {
        Utils.mostrarToast('Revisa los datos de pago.', 'warning');
        return;
      }
      renderResumen();
      irA(3);
    } else if (pasoActual === 3) {
      confirmarPago();
    }
  });

  btnVolver.addEventListener('click', () => {
    if (typeof pasoActual === 'number' && pasoActual > 1) {
      irA(pasoActual - 1);
    }
  });

  modalEl.addEventListener('show.bs.modal', () => {
    seleccionarMetodo();
    irA(1);
  });

  document.getElementById('btn-finalizar').addEventListener('click', () => {
    if (!Cart.items().length) return;
    modal.show();
  });

  document.addEventListener('carrito:actualizado', render);
  render();
});
