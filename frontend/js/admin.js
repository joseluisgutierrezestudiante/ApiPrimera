const baseUrl = (window.APP_CONFIG && window.APP_CONFIG.apiUrl) || 'http://localhost:5069/api';

async function verificarSesion() {
  try {
    const respuesta = await fetch(`${baseUrl}/auth/sesion`, { credentials: 'include' });
    if (respuesta.ok) return true;
  } catch (error) {
    /* API no disponible: se exige iniciar sesión de todos modos */
  }
  window.location.href = `./login.html?next=${encodeURIComponent('admin.html')}`;
  return false;
}

document.addEventListener('DOMContentLoaded', async () => {
  const autorizado = await verificarSesion();
  if (!autorizado) return;

  const tbody = document.getElementById('tabla-admin');
  const carga = document.getElementById('carga-admin');
  const alerta = document.getElementById('alerta-admin');
  const modal = new bootstrap.Modal(document.getElementById('modalProducto'));
  const form = document.getElementById('form-producto');
  const titulo = document.getElementById('modal-titulo');
  const btnEliminarImagen = document.getElementById('btn-eliminar-imagen');

  const campos = {
    id: document.getElementById('campo-id'),
    nombre: document.getElementById('campo-nombre'),
    marca: document.getElementById('campo-marca'),
    categoria: document.getElementById('campo-categoria'),
    anio: document.getElementById('campo-anio'),
    kilometraje: document.getElementById('campo-kilometraje'),
    combustible: document.getElementById('campo-combustible'),
    transmision: document.getElementById('campo-transmision'),
    color: document.getElementById('campo-color'),
    precio: document.getElementById('campo-precio'),
    precioOriginal: document.getElementById('campo-precio-original'),
    stock: document.getElementById('campo-stock'),
    descripcion: document.getElementById('campo-descripcion'),
    destacado: document.getElementById('campo-destacado'),
    imagen: document.getElementById('campo-imagen'),
  };

  let imagenActualUrl = null;

  function mostrarAlerta(mensaje, tipo = 'success') {
    alerta.innerHTML = `<div class="alert alert-${tipo} alert-dismissible fade show">
      ${Utils.escape(mensaje)}
      <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    </div>`;
  }

  async function cargarTabla() {
    carga.classList.remove('d-none');
    try {
      const productos = await ProductoAPI.listar({ orden: 'nombre-asc' });
      renderTabla(productos);
    } catch (error) {
      mostrarAlerta(error.message, 'danger');
    } finally {
      carga.classList.add('d-none');
    }
  }

  function renderTabla(productos) {
    tbody.innerHTML = productos.map((p) => `
      <tr>
        <td><img src="${Utils.escape(Utils.imagenSegura(p))}" alt="" width="60" height="42" class="rounded object-fit-cover"></td>
        <td>
          <div class="fw-semibold">${Utils.escape(p.nombre)}</div>
          <div class="text-muted small">${Utils.escape(p.categoria)} · ${Utils.escape(p.anio)}</div>
        </td>
        <td>${Utils.escape(p.marca)}</td>
        <td class="text-end">${Utils.formatoCOP(p.precio)}</td>
        <td class="text-center">${p.stock}</td>
        <td class="text-center">${p.destacado ? '⭐' : '—'}</td>
        <td class="text-end text-nowrap">
          <button class="btn btn-sm btn-outline-secondary" data-editar="${p.id}">Editar</button>
          <button class="btn btn-sm btn-outline-danger" data-eliminar="${p.id}">Eliminar</button>
        </td>
      </tr>`).join('');

    tbody.querySelectorAll('[data-editar]').forEach((boton) => {
      boton.addEventListener('click', () => abrirEdicion(productos.find((p) => p.id === Number(boton.dataset.editar))));
    });

    tbody.querySelectorAll('[data-eliminar]').forEach((boton) => {
      boton.addEventListener('click', () => eliminar(Number(boton.dataset.eliminar)));
    });
  }

  function limpiarFormulario() {
    form.reset();
    campos.id.value = '';
    campos.anio.value = 2024;
    campos.kilometraje.value = 0;
    campos.combustible.value = 'Gasolina';
    campos.transmision.value = 'Manual';
    campos.stock.value = 1;
    imagenActualUrl = null;
    btnEliminarImagen.classList.add('d-none');
  }

  function abrirNuevo() {
    limpiarFormulario();
    titulo.textContent = 'Nuevo vehículo';
    modal.show();
  }

  function abrirEdicion(producto) {
    if (!producto) return;
    limpiarFormulario();

    campos.id.value = producto.id;
    campos.nombre.value = producto.nombre;
    campos.marca.value = producto.marca;
    campos.categoria.value = producto.categoria;
    campos.anio.value = producto.anio;
    campos.kilometraje.value = producto.kilometraje;
    campos.combustible.value = producto.combustible;
    campos.transmision.value = producto.transmision;
    campos.color.value = producto.color;
    campos.precio.value = producto.precio;
    campos.precioOriginal.value = producto.precioOriginal ?? '';
    campos.stock.value = producto.stock;
    campos.descripcion.value = producto.descripcion;
    campos.destacado.checked = producto.destacado;
    imagenActualUrl = producto.imagenUrl || null;
    btnEliminarImagen.classList.toggle('d-none', !imagenActualUrl);

    titulo.textContent = `Editar: ${producto.nombre}`;
    modal.show();
  }

  function leerFormulario() {
    return {
      id: Number(campos.id.value) || 0,
      nombre: campos.nombre.value.trim(),
      marca: campos.marca.value.trim(),
      categoria: campos.categoria.value.trim(),
      anio: Number(campos.anio.value) || 0,
      kilometraje: Number(campos.kilometraje.value) || 0,
      combustible: campos.combustible.value.trim(),
      transmision: campos.transmision.value.trim(),
      color: campos.color.value.trim(),
      precio: Number(campos.precio.value) || 0,
      precioOriginal: campos.precioOriginal.value ? Number(campos.precioOriginal.value) : null,
      stock: Number(campos.stock.value) || 0,
      descripcion: campos.descripcion.value.trim(),
      destacado: campos.destacado.checked,
    };
  }

  async function subirImagenSiCorresponde(id) {
    const archivo = campos.imagen.files[0];
    if (!archivo) return;
    await ProductoAPI.subirImagen(id, archivo);
  }

  form.addEventListener('submit', async (evento) => {
    evento.preventDefault();
    const boton = document.getElementById('btn-guardar');
    boton.disabled = true;

    try {
      const datos = leerFormulario();
      const esNuevo = !datos.id;

      const guardado = esNuevo
        ? await ProductoAPI.crear(datos)
        : await ProductoAPI.actualizar(datos.id, datos);

      const id = esNuevo ? guardado.id : datos.id;
      await subirImagenSiCorresponde(id);

      modal.hide();
      mostrarAlerta(esNuevo ? 'Vehículo creado correctamente.' : 'Vehículo actualizado correctamente.');
      cargarTabla();
    } catch (error) {
      mostrarAlerta(error.message, 'danger');
    } finally {
      boton.disabled = false;
    }
  });

  async function eliminar(id) {
    if (!confirm('¿Eliminar este vehículo? Esta acción no se puede deshacer.')) return;
    try {
      await ProductoAPI.eliminar(id);
      mostrarAlerta('Vehículo eliminado.', 'warning');
      cargarTabla();
    } catch (error) {
      mostrarAlerta(error.message, 'danger');
    }
  }

  btnEliminarImagen.addEventListener('click', async () => {
    const id = Number(campos.id.value);
    if (!id) return;
    try {
      await ProductoAPI.eliminarImagen(id);
      imagenActualUrl = null;
      btnEliminarImagen.classList.add('d-none');
      mostrarAlerta('Imagen eliminada.', 'warning');
      cargarTabla();
    } catch (error) {
      mostrarAlerta(error.message, 'danger');
    }
  });

  document.getElementById('btn-nuevo').addEventListener('click', abrirNuevo);

  document.getElementById('btn-salir').addEventListener('click', async () => {
    try {
      await fetch(`${baseUrl}/auth/logout`, { method: 'POST', credentials: 'include' });
    } catch (error) {
      /* cerramos la sesión local aunque falle la llamada */
    }
    try {
      localStorage.removeItem('autoprime_usuario');
    } catch (e) {
      /* almacenamiento no disponible */
    }
    window.location.href = './login.html';
  });

  cargarTabla();
});
