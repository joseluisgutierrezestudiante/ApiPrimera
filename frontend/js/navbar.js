document.addEventListener('DOMContentLoaded', () => {
  const nav = document.querySelector('.navbar');
  const menuIcon =
    document.querySelector('.navbar-toggler .material-symbols-outlined') ||
    document.querySelector('.material-symbols-outlined');
  if (!nav || !menuIcon) return;

  menuIcon.addEventListener('click', () => {
    nav.classList.toggle('active');
  });

  const collapse = document.getElementById('navPrincipal');
  if (collapse) {
    collapse.addEventListener('shown.bs.collapse', () => nav.classList.add('active'));
    collapse.addEventListener('hidden.bs.collapse', () => nav.classList.remove('active'));
  }
});
