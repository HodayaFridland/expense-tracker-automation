const API = '/transactions';

async function loadTransactions() {
  const res = await fetch(API);
  const transactions = await res.json();
  renderTransactions(transactions);
  loadSummary();
}

function renderTransactions(transactions) {
  const body = document.getElementById('transactions-body');
  const empty = document.getElementById('empty-message');
  body.innerHTML = '';
  if (transactions.length === 0) {
    empty.textContent = 'No data yet';
    return;
  }
  empty.textContent = '';
  for (const t of transactions) {
    const row = document.createElement('tr');
    row.innerHTML =
      `<td>${t.date.substring(0, 10)}</td><td>${t.category}</td>` +
      `<td>${t.type}</td><td>${t.amount}</td>` +
      `<td><button onclick="deleteTransaction(${t.id})">Delete</button></td>`;
    body.appendChild(row);
  }
}

async function loadSummary() {
  const res = await fetch(API + '/summary');
  const s = await res.json();
  document.getElementById('total-income').textContent = s.income;
  document.getElementById('total-expenses').textContent = s.expenses;
  document.getElementById('balance').textContent = s.balance;
}

async function deleteTransaction(id) {
  await fetch(`${API}/${id}`, { method: 'DELETE' });
  loadTransactions();
}

document.getElementById('transaction-form').addEventListener('submit', async (e) => {
  e.preventDefault();
  const errorEl = document.getElementById('error-message');
  errorEl.textContent = '';

  const newTransaction = {
    amount: parseFloat(document.getElementById('amount-input').value),
    type: document.getElementById('type-input').value,
    category: document.getElementById('category-input').value,
    date: document.getElementById('date-input').value
  };

  const res = await fetch(API, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(newTransaction)
  });

  if (!res.ok) {
    errorEl.textContent = await res.text();
    return;
  }

  e.target.reset();
  loadTransactions();
});

loadTransactions();
