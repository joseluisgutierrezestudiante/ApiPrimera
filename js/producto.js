document.addEventListener('DOMContentLoaded', async () => {
  const carga = document.getElementById('estado-carga');
  const contenedor = document.getElementById('detalle-producto');
  const id = Number(Utils.parametroUrl('id'));

  if (!id) {
    carga.innerHTML = `
      <div class="alert alert-warning py-4 text-center">
        <h4 class="alert-heading fw-bold">Vehículo no especificado</h4>
        <p class="mb-3">Por favor selecciona un vehículo desde nuestro catálogo.</p>
        <a href="./index.html#catalogo" class="btn btn-accent">Ver catálogo</a>
      </div>`;
    return;
  }

  try {
    const producto = await ProductoAPI.obtener(id);

    if (!producto) {
      carga.innerHTML = `
        <div class="alert alert-warning py-4 text-center">
          <h4 class="alert-heading fw-bold">Vehículo no encontrado</h4>
          <p class="mb-3">El vehículo solicitado no está disponible en este momento.</p>
          <a href="./index.html#catalogo" class="btn btn-accent">Volver al catálogo</a>
        </div>`;
      return;
    }

    // Actualizar el título de la página
    document.title = `${producto.marca} ${producto.nombre} | AutoPrime`;

    contenedor.innerHTML = plantilla(producto);
    contenedor.classList.remove('d-none');

    // Inicializar componentes interactivos
    inicializarGaleria(producto);
    inicializarSimulador(producto);
    inicializarAcciones(producto);
  } catch (error) {
    carga.innerHTML = `
      <div class="alert alert-danger py-4 text-center">
        <strong>Error:</strong> ${Utils.escape(error.message)}
        <div class="mt-3">
          <a href="./index.html" class="btn btn-outline-secondary">Ir al inicio</a>
        </div>
      </div>`;
    return;
  }

  carga.classList.add('d-none');
});

function plantilla(producto) {
  const descuento = Number(producto.descuentoPorcentaje) || 0;
  const tieneOriginal = descuento > 0 && producto.precioOriginal;
  const agotado = Boolean(producto.agotado);
  const ahorro = tieneOriginal ? Number(producto.precioOriginal) - Number(producto.precio) : 0;

  // Galería de fotos (o arreglo con la principal si no hay más)
  const fotos = Array.isArray(producto.imagenes) && producto.imagenes.length
    ? producto.imagenes
    : [Utils.imagenSegura(producto)];

  const thumbsHtml = fotos.length > 1
    ? `<div class="galeria-thumbs">
        ${fotos.map((foto, idx) => `
          <button type="button" class="galeria-thumb ${idx === 0 ? 'active' : ''}" data-indice="${idx}" aria-label="Ver imagen ${idx + 1}">
            <img src="${Utils.escape(foto)}" alt="${Utils.escape(producto.nombre)} miniatura ${idx + 1}" loading="lazy">
          </button>
        `).join('')}
      </div>`
    : '';

  // Mensaje de WhatsApp personalizado
  const telefonoWhatsApp = '573001234567';
  const textoConsulta = `¡Hola AutoPrime! 🚗 Me interesa el vehículo *${producto.marca} ${producto.nombre}* (${producto.anio}) con precio de ${Utils.formatoCOP(producto.precio)}. ¿Sigue disponible para agendar una prueba de manejo o asesoría? (Ref: #${producto.id})`;
  const urlWhatsApp = `https://wa.me/${telefonoWhatsApp}?text=${encodeURIComponent(textoConsulta)}`;

  return `
    <!-- Breadcrumb de navegación -->
    <nav aria-label="breadcrumb" class="mb-4">
      <ol class="breadcrumb">
        <li class="breadcrumb-item"><a href="./index.html" class="text-decoration-none text-muted">Inicio</a></li>
        <li class="breadcrumb-item"><a href="./index.html#catalogo" class="text-decoration-none text-muted">Catálogo</a></li>
        <li class="breadcrumb-item text-muted">${Utils.escape(producto.marca)}</li>
        <li class="breadcrumb-item active text-danger fw-semibold" aria-current="page">${Utils.escape(producto.nombre)}</li>
      </ol>
    </nav>

    <div class="row g-5">
      <!-- Columna Multimedia / Galería -->
      <div class="col-lg-6">
        <div class="galeria-producto">
          <div class="galeria-principal-wrapper">
            <img id="img-principal" src="${Utils.escape(fotos[0])}" class="galeria-principal-img" alt="${Utils.escape(producto.nombre)}">
            ${descuento > 0 ? `<span class="badge-descuento">-${descuento}% OFF</span>` : ''}
            ${producto.destacado ? '<span class="badge-destacado">Destacado</span>' : ''}
            ${agotado ? '<div class="agotado-overlay">Agotado</div>' : ''}
          </div>
          ${thumbsHtml}
        </div>

        <!-- Badges de Confianza Automotriz -->
        <div class="trust-badges-grid">
          <div class="trust-badge-card">
            <div class="trust-badge-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22">
                <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
              </svg>
            </div>
            <div class="trust-badge-info">
              <h6>Peritaje Certificado</h6>
              <p>Revisión técnica de 100 puntos y antecedentes al día.</p>
            </div>
          </div>

          <div class="trust-badge-card">
            <div class="trust-badge-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22">
                <circle cx="12" cy="12" r="10"/>
                <polyline points="12 6 12 12 16 14"/>
              </svg>
            </div>
            <div class="trust-badge-info">
              <h6>Entrega Inmediata</h6>
              <p>Disponibilidad física en Bucaramanga y envíos nacionales.</p>
            </div>
          </div>

          <div class="trust-badge-card">
            <div class="trust-badge-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22">
                <path d="M21.5 2v6h-6M21.34 15.57a10 10 0 1 1-.57-8.38l5.67-5.67"/>
              </svg>
            </div>
            <div class="trust-badge-info">
              <h6>Retomamos tu Usado</h6>
              <p>Recibimos tu vehículo actual como parte de pago.</p>
            </div>
          </div>

          <div class="trust-badge-card">
            <div class="trust-badge-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22">
                <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/>
                <polyline points="14 2 14 8 20 8"/>
                <line x1="16" y1="13" x2="8" y2="13"/>
                <line x1="16" y1="17" x2="8" y2="17"/>
                <polyline points="10 9 9 9 8 9"/>
              </svg>
            </div>
            <div class="trust-badge-info">
              <h6>Garantía AutoPrime</h6>
              <p>1 año o 20.000 km de respaldo mecánico total.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Columna Comercial y Ficha Técnica -->
      <div class="col-lg-6">
        <div class="d-flex align-items-center gap-2 mb-2">
          <span class="badge bg-dark text-uppercase px-2 py-1">${Utils.escape(producto.marca)}</span>
          <span class="badge bg-secondary-subtle text-dark text-uppercase px-2 py-1">${Utils.escape(producto.categoria)}</span>
          ${producto.kilometraje === 0 ? '<span class="badge bg-success-subtle text-success px-2 py-1">0 km · Nuevo</span>' : ''}
        </div>

        <h1 class="h2 fw-bold text-dark">${Utils.escape(producto.nombre)}</h1>

        <!-- Bloque de Precio y Ahorro -->
        <div class="d-flex align-items-baseline gap-3 my-3 flex-wrap">
          <span class="precio-actual fs-2 fw-bold">${Utils.formatoCOP(producto.precio)}</span>
          ${tieneOriginal ? `<span class="precio-original fs-5">${Utils.formatoCOP(producto.precioOriginal)}</span>` : ''}
          ${ahorro > 0 ? `<span class="badge bg-danger-subtle text-danger fw-semibold">Ahorras ${Utils.formatoCOP(ahorro)}</span>` : ''}
        </div>

        <p class="text-muted lead fs-6">${Utils.escape(producto.descripcion)}</p>

        <!-- Ficha de Especificaciones Técnicas -->
        <div class="row row-cols-2 g-3 my-3">
          ${dato('Año de fabricación', producto.anio)}
          ${dato('Kilometraje', producto.kilometraje > 0 ? `${Utils.formatoNumero(producto.kilometraje)} km` : '0 km (Nuevo)')}
          ${dato('Combustible', producto.combustible || 'Gasolina')}
          ${dato('Transmisión', producto.transmision || 'Automática')}
          ${dato('Color exterior', producto.color || 'A convenir')}
          ${dato('Disponibilidad', agotado ? 'Agotado' : `${producto.stock} unidades en sala`)}
        </div>

        <!-- Botones de Acción (WhatsApp + Carrito) -->
        <div class="d-flex flex-column gap-3 mt-4">
          <!-- Botón Directo WhatsApp -->
          <a href="${urlWhatsApp}" target="_blank" rel="noopener noreferrer" class="btn btn-whatsapp btn-lg shadow-sm py-3">
            <svg viewBox="0 0 24 24" fill="currentColor" width="22" height="22">
              <path d="M12 2a10 10 0 0 0-8.6 15l-1.3 4.7 4.8-1.3A10 10 0 1 0 12 2zm0 18a8 8 0 0 1-4.1-1.1l-.3-.2-2.8.8.8-2.8-.2-.3A8 8 0 1 1 12 20zm4.4-5.9c-.2-.1-1.4-.7-1.6-.8-.2-.1-.4-.1-.5.1l-.7.9c-.1.2-.3.2-.5.1a6.5 6.5 0 0 1-3.2-2.8c-.1-.2 0-.4.1-.5l.4-.5c.1-.2.1-.3 0-.5l-.7-1.7c-.2-.4-.4-.4-.5-.4h-.5c-.2 0-.5.1-.7.3-.2.2-.9.9-.9 2.1s.9 2.4 1 2.6c.1.2 1.8 2.7 4.3 3.8.6.3 1.1.4 1.5.5.6.2 1.2.2 1.6.1.5-.1 1.4-.6 1.6-1.1.2-.5.2-1 .1-1.1-.1-.1-.2-.2-.4-.3z"/>
            </svg>
            <span>Consultar por WhatsApp con un Asesor</span>
          </a>

          <!-- Botón Carrito / Reserva Online -->
          <div class="d-flex gap-2 align-items-center">
            <input id="cantidad" type="number" class="form-control form-control-lg text-center" value="1" min="1" max="${producto.stock || 5}" style="max-width: 5rem" title="Cantidad">
            <button id="btn-agregar" class="btn btn-accent btn-lg flex-grow-1" ${agotado ? 'disabled' : ''}>
              ${agotado ? 'Vehículo agotado' : 'Agregar al carrito / Reservar'}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- SECCIÓN: SIMULADOR DE FINANCIACIÓN AUTOMOTRIZ -->
    <section class="simulador-card">
      <div class="simulador-header d-flex flex-wrap justify-content-between align-items-center gap-2">
        <div>
          <span class="badge bg-danger-subtle text-danger fw-bold text-uppercase mb-1">Crédito vehicular</span>
          <h3 class="h4 fw-bold mb-1">Simula tu cuota mensual estimada</h3>
          <p class="text-muted small mb-0">Ajusta la cuota inicial y el plazo para calcular tu plan de financiación ideal.</p>
        </div>
        <div class="text-end">
          <span class="badge bg-light text-muted border">Tasa referencial: 1.19% M.V.</span>
        </div>
      </div>

      <div class="row g-4 align-items-stretch">
        <!-- Controles del Simulador -->
        <div class="col-lg-7">
          <!-- Slider Cuota Inicial -->
          <div class="simulador-slider-control">
            <div class="d-flex justify-content-between align-items-baseline mb-2">
              <label for="sim-inicial-slider" class="form-label fw-semibold mb-0">Cuota inicial:</label>
              <div class="text-end">
                <span id="sim-inicial-pct" class="badge bg-dark fs-6 me-2">20%</span>
                <span id="sim-inicial-monto" class="fw-bold text-danger fs-5">$0</span>
              </div>
            </div>
            <input type="range" class="form-range simulador-slider" id="sim-inicial-slider" min="10" max="70" step="5" value="20">
            <div class="d-flex justify-content-between text-muted small mt-1">
              <span>Mínimo: 10%</span>
              <span>50%</span>
              <span>Máximo: 70%</span>
            </div>
          </div>

          <!-- Selector de Plazo (Meses) -->
          <div>
            <label class="form-label fw-semibold mb-2">Plazo en meses:</label>
            <div class="plazo-chips" id="sim-plazos">
              <button type="button" class="plazo-chip" data-plazo="12">12 meses</button>
              <button type="button" class="plazo-chip" data-plazo="24">24 meses</button>
              <button type="button" class="plazo-chip" data-plazo="36">36 meses</button>
              <button type="button" class="plazo-chip" data-plazo="48">48 meses</button>
              <button type="button" class="plazo-chip active" data-plazo="60">60 meses</button>
              <button type="button" class="plazo-chip" data-plazo="72">72 meses</button>
            </div>
          </div>
        </div>

        <!-- Tarjeta de Resultados -->
        <div class="col-lg-5">
          <div class="simulador-resultado-box">
            <div>
              <span class="text-white-50 text-uppercase small fw-semibold">Cuota mensual estimada</span>
              <div class="simulador-cuota-monto mt-1" id="sim-cuota-resultado">
                $0<small>/mes</small>
              </div>

              <div class="mt-3">
                <div class="simulador-linea">
                  <span>Precio del vehículo:</span>
                  <span id="sim-precio-vehiculo">${Utils.formatoCOP(producto.precio)}</span>
                </div>
                <div class="simulador-linea">
                  <span>Cuota inicial:</span>
                  <span id="sim-resumen-inicial">$0</span>
                </div>
                <div class="simulador-linea">
                  <span>Monto a financiar:</span>
                  <span id="sim-monto-financiar">$0</span>
                </div>
                <div class="simulador-linea">
                  <span>Plazo:</span>
                  <span id="sim-resumen-plazo">60 meses</span>
                </div>
              </div>
            </div>

            <div class="mt-4 pt-3 border-top border-secondary">
              <button type="button" id="btn-solicitar-credito" class="btn btn-whatsapp w-100 py-2">
                <svg viewBox="0 0 24 24" fill="currentColor" width="18" height="18">
                  <path d="M12 2a10 10 0 0 0-8.6 15l-1.3 4.7 4.8-1.3A10 10 0 1 0 12 2zm0 18a8 8 0 0 1-4.1-1.1l-.3-.2-2.8.8.8-2.8-.2-.3A8 8 0 1 1 12 20zm4.4-5.9c-.2-.1-1.4-.7-1.6-.8-.2-.1-.4-.1-.5.1l-.7.9c-.1.2-.3.2-.5.1a6.5 6.5 0 0 1-3.2-2.8c-.1-.2 0-.4.1-.5l.4-.5c.1-.2.1-.3 0-.5l-.7-1.7c-.2-.4-.4-.4-.5-.4h-.5c-.2 0-.5.1-.7.3-.2.2-.9.9-.9 2.1s.9 2.4 1 2.6c.1.2 1.8 2.7 4.3 3.8.6.3 1.1.4 1.5.5.6.2 1.2.2 1.6.1.5-.1 1.4-.6 1.6-1.1.2-.5.2-1 .1-1.1-.1-.1-.2-.2-.4-.3z"/>
                </svg>
                <span>Solicitar este plan de crédito</span>
              </button>
              <p class="text-white-50 text-center mb-0 mt-2" style="font-size: 0.72rem">
                *Simulación orientativa. Sujeta a estudio de crédito bancario.
              </p>
            </div>
          </div>
        </div>
      </div>
    </section>`;
}

function dato(etiqueta, valor) {
  return `
    <div class="col">
      <div class="border rounded-3 p-3 h-100 superficie">
        <div class="text-muted small">${Utils.escape(etiqueta)}</div>
        <div class="fw-semibold text-dark">${Utils.escape(valor)}</div>
      </div>
    </div>`;
}

/**
 * Galería interactiva con cambio de fotos y transición suave.
 */
function inicializarGaleria(producto) {
  const fotos = Array.isArray(producto.imagenes) && producto.imagenes.length
    ? producto.imagenes
    : [Utils.imagenSegura(producto)];

  const imgPrincipal = document.getElementById('img-principal');
  const thumbs = document.querySelectorAll('.galeria-thumb');

  if (!imgPrincipal || !thumbs.length) return;

  thumbs.forEach((thumb) => {
    thumb.addEventListener('click', () => {
      const idx = Number(thumb.dataset.indice);
      if (fotos[idx]) {
        // Transición de opacidad
        imgPrincipal.style.opacity = '0.3';
        setTimeout(() => {
          imgPrincipal.src = fotos[idx];
          imgPrincipal.style.opacity = '1';
        }, 150);

        thumbs.forEach((t) => t.classList.remove('active'));
        thumb.classList.add('active');
      }
    });
  });
}

/**
 * Simulador de crédito vehicular en tiempo real.
 */
function inicializarSimulador(producto) {
  const precio = Number(producto.precio) || 0;
  if (!precio) return;

  const slider = document.getElementById('sim-inicial-slider');
  const pctLabel = document.getElementById('sim-inicial-pct');
  const montoInicialLabel = document.getElementById('sim-inicial-monto');
  const cuotaResultado = document.getElementById('sim-cuota-resultado');
  const resumenInicial = document.getElementById('sim-resumen-inicial');
  const resumenFinanciar = document.getElementById('sim-monto-financiar');
  const resumenPlazo = document.getElementById('sim-resumen-plazo');
  const plazosContainer = document.getElementById('sim-plazos');
  const btnSolicitarCredito = document.getElementById('btn-solicitar-credito');

  if (!slider || !plazosContainer) return;

  let plazoActual = 60; // Plazo predeterminado: 60 meses
  const TASA_MENSUAL = 0.0119; // 1.19% mensual vencido

  function recalcular() {
    const pct = Number(slider.value) || 20;
    const cuotaInicial = Math.round(precio * (pct / 100));
    const montoFinanciar = Math.max(0, precio - cuotaInicial);

    // Fórmula financiera de cuota fija (método francés)
    let cuotaMensual = 0;
    if (montoFinanciar > 0 && plazoActual > 0) {
      const factor = Math.pow(1 + TASA_MENSUAL, plazoActual);
      cuotaMensual = Math.round(montoFinanciar * ((TASA_MENSUAL * factor) / (factor - 1)));
    }

    // Actualizar UI
    pctLabel.textContent = `${pct}%`;
    montoInicialLabel.textContent = Utils.formatoCOP(cuotaInicial);
    resumenInicial.textContent = `${Utils.formatoCOP(cuotaInicial)} (${pct}%)`;
    resumenFinanciar.textContent = Utils.formatoCOP(montoFinanciar);
    resumenPlazo.textContent = `${plazoActual} meses`;
    cuotaResultado.innerHTML = `${Utils.formatoCOP(cuotaMensual)}<small>/mes</small>`;

    // Mensaje para el botón de WhatsApp de crédito
    if (btnSolicitarCredito) {
      const mensaje = `¡Hola AutoPrime! Deseo solicitar el estudio de crédito para el *${producto.marca} ${producto.nombre}*:\n` +
        `• Valor vehículo: ${Utils.formatoCOP(precio)}\n` +
        `• Cuota inicial propuesta: ${pct}% (${Utils.formatoCOP(cuotaInicial)})\n` +
        `• Plazo: ${plazoActual} meses\n` +
        `• Cuota estimada calculada: ${Utils.formatoCOP(cuotaMensual)}/mes\n\n` +
        `¿Qué documentos requiero presentar para radicar mi solicitud?`;

      btnSolicitarCredito.onclick = () => {
        window.open(`https://wa.me/573001234567?text=${encodeURIComponent(mensaje)}`, '_blank');
      };
    }
  }

  // Listener para el slider de cuota inicial
  slider.addEventListener('input', recalcular);

  // Listeners para los chips de plazo
  const chipButtons = plazosContainer.querySelectorAll('.plazo-chip');
  chipButtons.forEach((btn) => {
    btn.addEventListener('click', () => {
      chipButtons.forEach((b) => b.classList.remove('active'));
      btn.classList.add('active');
      plazoActual = Number(btn.dataset.plazo) || 60;
      recalcular();
    });
  });

  // Ejecución inicial
  recalcular();
}

/**
 * Acciones de carrito y agregar.
 */
function inicializarAcciones(producto) {
  const input = document.getElementById('cantidad');
  const botonAgregar = document.getElementById('btn-agregar');

  if (!botonAgregar) return;

  botonAgregar.addEventListener('click', () => {
    const cantidad = Math.max(1, Number(input.value) || 1);
    Cart.agregar(producto, cantidad);

    // Feedback visual en el botón
    const originalText = botonAgregar.innerHTML;
    botonAgregar.innerHTML = '✓ ¡Agregado al carrito!';
    botonAgregar.classList.replace('btn-accent', 'btn-success');

    Utils.mostrarToast(`¡${producto.nombre} agregado al carrito!`, 'success');

    setTimeout(() => {
      botonAgregar.innerHTML = originalText;
      botonAgregar.classList.replace('btn-success', 'btn-accent');
    }, 2000);
  });
}
