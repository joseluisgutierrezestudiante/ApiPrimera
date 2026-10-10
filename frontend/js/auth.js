const Auth = (() => {
  const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  const baseUrl = (window.APP_CONFIG && window.APP_CONFIG.apiUrl) || 'http://localhost:5069/api';

  function marcar(input, valido, mensaje) {
    if (!input) return valido;
    input.classList.toggle('is-invalid', !valido);
    const feedback = input.parentElement.querySelector('.invalid-feedback');
    if (feedback && mensaje) feedback.textContent = mensaje;
    return valido;
  }

  function validarEmail(input) {
    const valor = input.value.trim();
    if (!valor) return marcar(input, false, 'El correo electrónico es obligatorio.');
    if (!EMAIL_REGEX.test(valor)) return marcar(input, false, 'El formato del correo no es válido.');
    return marcar(input, true);
  }

  function extraerMensaje(contenido) {
    if (!contenido) return '';
    if (typeof contenido === 'string') return contenido;
    const bolsas = contenido.errores || contenido.errors;
    if (bolsas) {
      const lista = Object.keys(bolsas).flatMap((clave) => [].concat(bolsas[clave]));
      if (lista.length) return lista.join(' ');
    }
    return contenido.mensaje || contenido.detail || contenido.title || '';
  }

  function bloquearBoton(form, texto) {
    const boton = form.querySelector('button[type="submit"]');
    if (!boton) return () => {};
    const original = boton.innerHTML;
    boton.disabled = true;
    boton.textContent = texto;
    return () => {
      boton.disabled = false;
      boton.innerHTML = original;
    };
  }

  // Solo se acepta un archivo relativo simple (p. ej. "admin.html"): evita
  // redirecciones a otros dominios o rutas (open redirect).
  function destinoSeguro(valor) {
    if (typeof valor !== 'string') return null;
    return /^[a-zA-Z0-9_-]+\.html$/.test(valor) ? `./${valor}` : null;
  }

  function destinoPostLogin() {
    const siguiente = new URLSearchParams(window.location.search).get('next');
    return destinoSeguro(siguiente) || './index.html';
  }

  function initRegistro() {
    const form = document.getElementById('form-registro');
    if (!form) return;

    const nombre = document.getElementById('registro-nombre');
    const email = document.getElementById('registro-email');
    const password = document.getElementById('registro-password');
    const confirmar = document.getElementById('registro-confirmar');

    form.addEventListener('submit', async (evento) => {
      evento.preventDefault();
      let valido = true;

      valido = marcar(
        nombre,
        nombre.value.trim().length >= 3,
        'El nombre debe tener al menos 3 caracteres.',
      ) && valido;

      valido = validarEmail(email) && valido;

      valido = marcar(
        password,
        password.value.length >= 8,
        'La contraseña debe tener al menos 8 caracteres.',
      ) && valido;

      valido = marcar(
        confirmar,
        confirmar.value.length > 0 && confirmar.value === password.value,
        'Las contraseñas no coinciden.',
      ) && valido;

      if (!valido) {
        Utils.mostrarToast('Revisa los datos marcados.', 'warning');
        return;
      }

      const restaurar = bloquearBoton(form, 'Registrando...');
      try {
        const respuesta = await fetch(`${baseUrl}/auth/registro`, {
          method: 'POST',
          credentials: 'include',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            nombre: nombre.value.trim(),
            email: email.value.trim(),
            password: password.value,
            confirmarPassword: confirmar.value,
          }),
        });
        const contenido = await respuesta.json().catch(() => null);

        if (!respuesta.ok) {
          throw new Error(extraerMensaje(contenido) || `No se pudo completar el registro (${respuesta.status}).`);
        }

        Utils.mostrarToast(extraerMensaje(contenido) || 'Registro exitoso. Ahora inicia sesión.', 'success');
        setTimeout(() => {
          window.location.href = './login.html';
        }, 1200);
      } catch (error) {
        Utils.mostrarToast(error.message, 'danger');
        restaurar();
      }
    });
  }

  function initLogin() {
    const form = document.getElementById('form-login');
    if (!form) return;

    const email = document.getElementById('login-email');
    const password = document.getElementById('login-password');

    form.addEventListener('submit', async (evento) => {
      evento.preventDefault();
      let valido = true;

      valido = validarEmail(email) && valido;
      valido = marcar(password, password.value.length > 0, 'Ingresa tu contraseña.') && valido;

      if (!valido) {
        Utils.mostrarToast('Revisa los datos marcados.', 'warning');
        return;
      }

      const restaurar = bloquearBoton(form, 'Ingresando...');
      try {
        const respuesta = await fetch(`${baseUrl}/auth/login`, {
          method: 'POST',
          credentials: 'include',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            email: email.value.trim(),
            password: password.value,
          }),
        });
        const contenido = await respuesta.json().catch(() => null);

        if (!respuesta.ok) {
          throw new Error(extraerMensaje(contenido) || 'Correo o contraseña incorrectos.');
        }

        try {
          localStorage.setItem('autoprime_usuario', contenido?.email || email.value.trim());
        } catch (e) {
          /* almacenamiento no disponible */
        }

        Utils.mostrarToast(extraerMensaje(contenido) || 'Sesión iniciada.', 'success');
        setTimeout(() => {
          window.location.href = destinoPostLogin();
        }, 900);
      } catch (error) {
        Utils.mostrarToast(error.message, 'danger');
        restaurar();
      }
    });
  }

  document.addEventListener('DOMContentLoaded', () => {
    initRegistro();
    initLogin();
  });

  return { marcar, validarEmail };
})();
