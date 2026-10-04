const search = document.querySelector('#demoSearch');
const cards = [...document.querySelectorAll('.demo-card')];
const empty = document.querySelector('#demoEmpty');

search.addEventListener('input', () => {
  const query = search.value.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase().trim();
  let shown = 0;
  for (const card of cards) {
    const name = card.dataset.name.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
    card.hidden = !name.includes(query);
    if (!card.hidden) shown++;
  }
  empty.hidden = shown > 0;
});

const transparency = document.querySelector('#transparency');
const value = document.querySelector('#transparencyValue');
transparency.addEventListener('input', () => {
  const percent = Number(transparency.value);
  document.documentElement.style.setProperty('--transparency', String(percent / 100));
  value.textContent = `${percent}%`;
});
