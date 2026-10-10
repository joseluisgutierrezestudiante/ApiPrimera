/**
 * Efectos visuales del sitio AutoPrime:
 * 1) Navbar con sombra al desplazarse.
 * 2) Botón flotante "volver arriba".
 * 3) Aparición suave de secciones al entrar en pantalla (reveal),
 *    cubriendo también el contenido inyectado dinámicamente.
 */
document.addEventListener('DOMContentLoaded', () => {
  const navbar = document.querySelector('.navbar-autoprime');

  // ---------- 2) Botón volver arriba ----------
  const botonArriba = document.createElement('button');
  botonArriba.type = 'button';
  botonArriba.className = 'btn-volver-arriba';
  botonArriba.setAttribute('aria-label', 'Volver arriba');
  botonArriba.title = 'Volver arriba';
  botonArriba.innerHTML =
    '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 19V5"></path><path d="M5 12l7-7 7 7"></path></svg>';
  document.body.appendChild(botonArriba);

  botonArriba.addEventListener('click', () => {
    window.scrollTo({ top: 0, behavior: 'smooth' });
  });

  // ---------- 1) Navbar con sombra ----------
  const alDesplazar = () => {
    const y = window.scrollY;
    if (navbar) navbar.classList.toggle('scrolled', y > 16);
    botonArriba.classList.toggle('is-visible', y > 480);
  };

  window.addEventListener('scroll', alDesplazar, { passive: true });
  alDesplazar();

  // ---------- 3) Reveal de secciones ----------
  if (!('IntersectionObserver' in window)) return;

  const objetivos =
    '.banner-destacado, .hero-marcas, .footer-fila, .trust-badges-grid, .simulador-card, .auth-panel, .superficie';

  const observador = new IntersectionObserver(
    (entradas) => {
      entradas.forEach((entrada) => {
        if (entrada.isIntersecting) {
          entrada.target.classList.add('is-in');
          observador.unobserve(entrada.target);
        }
      });
    },
    { threshold: 0.12 }
  );

  const marcar = (elemento) => {
    if (!elemento || elemento.classList.contains('reveal')) return;
    elemento.classList.add('reveal');
    observador.observe(elemento);
  };

  const observarEn = (raiz) => {
    if (raiz.matches && raiz.matches(objetivos)) marcar(raiz);
    if (!raiz.querySelectorAll) return;
    raiz.querySelectorAll(objetivos).forEach(marcar);
  };

  observarEn(document);

  // Captura elementos inyectados después del load (cards, detalle, tablas…)
  new MutationObserver((registros) => {
    registros.forEach((registro) => {
      registro.addedNodes.forEach((nodo) => {
        if (nodo.nodeType === 1) observarEn(nodo);
      });
    });
  }).observe(document.body, { childList: true, subtree: true });
});
