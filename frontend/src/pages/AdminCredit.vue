
<template>
  <div class="admin-layout">
    <aside class="sidebar">
      <h2>FIFA Admin</h2>

      <nav>
        <router-link to="/admin">Dashboard</router-link>
        <router-link to="/admin/members">Members</router-link>
        <router-link to="/admin/answers">Answers</router-link>
        <router-link to="/admin/credit" class="active">Credit</router-link>
        <router-link to="/admin/export">Export</router-link>
      </nav>
    </aside>

    <main class="main-content">
      <div class="top-bar">
        <div>
          <h1>Credit Management</h1>
          <p>Add or deduct member credit and review transaction logs.</p>
        </div>

        <button type="button" class="logout-button" @click="logout">
          Logout
        </button>
      </div>

      <div v-if="errorMessage" class="card error">
        {{ errorMessage }}
      </div>

      <div v-if="successMessage" class="card success">
        {{ successMessage }}
      </div>

      <div v-if="lastCreditResult" class="balance-result-card">
        <h3>Latest Credit Operation Result</h3>

        <div class="balance-result-grid">
          <div>
            <span>Member ID</span>
            <strong>{{ lastCreditResult.memberId }}</strong>
          </div>

          <div>
            <span>Type</span>
            <strong>{{ lastCreditResult.transactionType }}</strong>
          </div>

          <div>
            <span>Amount</span>
            <strong>{{ formatAmount(lastCreditResult.amount) }}</strong>
          </div>

          <div>
            <span>Balance Before</span>
            <strong>{{ formatAmount(lastCreditResult.balanceBefore) }}</strong>
          </div>

          <div>
            <span>Balance After</span>
            <strong>{{ formatAmount(lastCreditResult.balanceAfter) }}</strong>
          </div>

          <div>
            <span>Current Balance</span>
            <strong>{{ formatAmount(lastCreditResult.balanceAfter) }}</strong>
          </div>
        </div>
      </div>

      <section class="form-card">
        <h2>Add / Deduct Credit</h2>

        <div class="form-grid">
          <div class="form-group">
            <label>Member ID</label>
            <input
              v-model.number="memberId"
              type="number"
              placeholder="Enter member ID"
            />
          </div>

          <div class="form-group">
            <label>Amount</label>
            <input
              v-model.number="amount"
              type="number"
              min="0"
              step="0.01"
              placeholder="Enter amount"
            />
          </div>

          <div class="form-group full-width">
            <label>Remark</label>
            <input
              v-model="remark"
              type="text"
              placeholder="Enter remark"
            />
          </div>
        </div>

        <div class="button-row">
          <button
            type="button"
            class="add-button"
            :disabled="submitting"
            @click="changeCredit('add')"
          >
            {{ submitting ? 'Processing...' : 'Add Credit' }}
          </button>

          <button
            type="button"
            class="deduct-button"
            :disabled="submitting"
            @click="changeCredit('deduct')"
          >
            {{ submitting ? 'Processing...' : 'Deduct Credit' }}
          </button>
        </div>
      </section>

      <section class="table-card">
        <div class="section-header">
          <div>
            <h2>Credit Transaction Log</h2>
            <p>Latest add / deduct / unlock credit records.</p>
          </div>

          <button type="button" class="refresh-button" @click="loadTransactions(filterMemberId)">
            Refresh
          </button>
        </div>

        <div class="transaction-filter">
          <input
            v-model="filterMemberId"
            type="number"
            placeholder="Filter by Member ID"
            @keyup.enter="searchTransactions"
          />

          <button type="button" class="search-button" @click="searchTransactions">
            Search
          </button>

          <button type="button" class="reset-button" @click="resetTransactionFilter">
            Reset
          </button>
        </div>

        <div class="table-scroll">
          <table>
            <thead>
              <tr>
                <th>Transaction ID</th>
                <th>Member ID</th>
                <th>Username</th>
                <th>Type</th>
                <th>Amount</th>
                <th>Before</th>
                <th>After</th>
                <th>Remark</th>
                <th>Admin</th>
                <th>Created At</th>
              </tr>
            </thead>

            <tbody>
              <tr v-if="transactions.length === 0">
                <td colspan="10" class="empty">
                  No credit transactions found.
                </td>
              </tr>

              <tr
                v-for="transaction in transactions"
                :key="transaction.transactionId"
              >
                <td>{{ transaction.transactionId }}</td>
                <td>{{ transaction.memberId }}</td>
                <td>{{ transaction.username }}</td>
                <td>
                  <span :class="getTransactionTypeClass(transaction.transactionType)">
                    {{ transaction.transactionType }}
                  </span>
                </td>
                <td>{{ formatAmount(transaction.amount) }}</td>
                <td>{{ formatAmount(transaction.balanceBefore) }}</td>
                <td>{{ formatAmount(transaction.balanceAfter) }}</td>
                <td class="remark-cell">{{ transaction.remark || '-' }}</td>
                <td>{{ transaction.adminUsername || '-' }}</td>
                <td class="date-cell">{{ formatDateTime(transaction.createdAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const API_BASE_URL = 'https://localhost:7160'

const memberId = ref(1)
const amount = ref(0)
const remark = ref('')
const submitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')
const transactions = ref([])
const filterMemberId = ref('')
const lastCreditResult = ref(null)

function checkAdminLogin() {
  const adminId = localStorage.getItem('adminId')

  if (!adminId) {
    router.push('/admin/login')
    return false
  }

  return true
}

function getAdminId() {
  return Number(localStorage.getItem('adminId') || 0)
}

async function changeCredit(action) {
  try {
    errorMessage.value = ''
    successMessage.value = ''
    lastCreditResult.value = null

    if (!memberId.value || memberId.value <= 0) {
      errorMessage.value = 'Member ID is required.'
      return
    }

    if (!amount.value || amount.value <= 0) {
      errorMessage.value = 'Amount must be greater than 0.'
      return
    }

    submitting.value = true

    const response = await fetch(`${API_BASE_URL}/api/AdminCredit/${action}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        memberId: memberId.value,
        amount: amount.value,
        remark: remark.value.trim(),
        adminId: getAdminId(),
      }),
    })

    const result = await response.json()

    if (!response.ok || result.status !== 'OK') {
      errorMessage.value = result.message || 'Failed to change credit.'
      return
    }

    successMessage.value = result.message
    lastCreditResult.value = result.data || null

    amount.value = 0
    remark.value = ''

    await loadTransactions(filterMemberId.value)
  } catch (error) {
    errorMessage.value = error.message || 'Unexpected error occurred.'
  } finally {
    submitting.value = false
  }
}

async function loadTransactions(memberIdFilter = '') {
  try {
    errorMessage.value = ''

    const query = memberIdFilter
      ? `?memberId=${memberIdFilter}`
      : ''

    const response = await fetch(`${API_BASE_URL}/api/AdminCredit/transactions${query}`)
    const result = await response.json()

    if (!response.ok || result.status !== 'OK') {
      throw new Error(result.message || 'Failed to load credit transactions.')
    }

    transactions.value = result.data || []
  } catch (error) {
    errorMessage.value = error.message || 'Failed to load credit transactions.'
  }
}

async function searchTransactions() {
  if (!filterMemberId.value) {
    await loadTransactions()
    return
  }

  await loadTransactions(filterMemberId.value)
}

async function resetTransactionFilter() {
  filterMemberId.value = ''
  await loadTransactions()
}

function logout() {
  localStorage.removeItem('adminId')
  localStorage.removeItem('adminUsername')
  localStorage.removeItem('adminDisplayName')

  router.push('/admin/login')
}

function getTransactionTypeClass(transactionType) {
  if (transactionType === 'ADD') {
    return 'type add'
  }

  if (transactionType === 'DEDUCT') {
    return 'type deduct'
  }

  if (transactionType === 'UNLOCK') {
    return 'type unlock'
  }

  return 'type default'
}

function formatAmount(value) {
  if (value === null || value === undefined) {
    return '0.00'
  }

  return Number(value).toFixed(2)
}

function formatDateTime(value) {
  if (!value) {
    return '-'
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString()
}

onMounted(async () => {
  if (!checkAdminLogin()) {
    return
  }

  await loadTransactions()
})
</script>
<style scoped>
.admin-layout {
  min-height: 100vh;
  display: flex;
  background: #f3f4f6;
  font-family: Arial, sans-serif;
}

.sidebar {
  width: 240px;
  padding: 24px;
  background: #111827;
  color: white;
  flex-shrink: 0;
}

.sidebar h2 {
  margin: 0 0 28px;
}

nav {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

nav a {
  padding: 10px 12px;
  border-radius: 6px;
  color: #d1d5db;
  text-decoration: none;
  cursor: pointer;
}

nav a.active,
nav a:hover {
  background: #1f2937;
  color: white;
}

.main-content {
  flex: 1;
  padding: 32px 40px;
  overflow-x: auto;
}

.top-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.top-bar h1 {
  margin: 0;
  color: #111827;
}

.top-bar p {
  margin: 6px 0 0;
  color: #6b7280;
}

.logout-button {
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  background: #dc2626;
  color: white;
  cursor: pointer;
}

.card,
.form-card,
.table-card {
  margin-top: 28px;
  padding: 20px;
  border-radius: 10px;
  background: white;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
}

.table-card {
  overflow: hidden;
}

.error {
  color: #dc2626;
  text-align: center;
}

.success {
  color: #166534;
  text-align: center;
}

.balance-result-card {
  margin-top: 28px;
  padding: 20px;
  border-radius: 10px;
  background: #ecfdf5;
  border: 1px solid #bbf7d0;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
}

.balance-result-card h3 {
  margin: 0 0 16px;
  color: #166534;
}

.balance-result-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 14px;
}

.balance-result-grid div {
  padding: 14px;
  border-radius: 8px;
  background: white;
}

.balance-result-grid span {
  display: block;
  margin-bottom: 6px;
  color: #6b7280;
  font-size: 13px;
}

.balance-result-grid strong {
  color: #111827;
  font-size: 18px;
}

.form-card h2,
.table-card h2 {
  margin: 0 0 16px;
  color: #111827;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.full-width {
  grid-column: 1 / 3;
}

label {
  margin-bottom: 6px;
  font-weight: bold;
  color: #374151;
}

input {
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
}

.button-row {
  display: flex;
  gap: 12px;
  margin-top: 20px;
}

.add-button,
.deduct-button,
.refresh-button,
.search-button,
.reset-button {
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  color: white;
  cursor: pointer;
}

.add-button {
  background: #16a34a;
}

.deduct-button {
  background: #dc2626;
}

.refresh-button,
.search-button {
  background: #2563eb;
}

.reset-button {
  background: #6b7280;
}

.add-button:disabled,
.deduct-button:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.section-header p {
  margin: 4px 0 0;
  color: #6b7280;
}

.transaction-filter {
  display: flex;
  gap: 12px;
  margin-top: 18px;
  padding: 14px;
  border-radius: 8px;
  background: #f9fafb;
}

.transaction-filter input {
  width: 220px;
}

.table-scroll {
  width: 100%;
  margin-top: 18px;
  overflow-x: auto;
  padding-bottom: 8px;
}

table {
  width: 100%;
  min-width: 1080px;
  border-collapse: collapse;
  font-size: 14px;
  table-layout: fixed;
}

th {
  padding: 12px;
  background: #f9fafb;
  border-bottom: 1px solid #e5e7eb;
  text-align: left;
  color: #374151;
  white-space: nowrap;
}

td {
  padding: 12px;
  border-bottom: 1px solid #e5e7eb;
  color: #111827;
  vertical-align: top;
}

th:nth-child(1),
td:nth-child(1) {
  width: 120px;
  min-width: 120px;
}

th:nth-child(2),
td:nth-child(2) {
  width: 100px;
  min-width: 100px;
}

th:nth-child(3),
td:nth-child(3) {
  width: 130px;
  min-width: 130px;
}

th:nth-child(4),
td:nth-child(4) {
  width: 110px;
  min-width: 110px;
}

th:nth-child(5),
td:nth-child(5),
th:nth-child(6),
td:nth-child(6),
th:nth-child(7),
td:nth-child(7) {
  width: 100px;
  min-width: 100px;
  text-align: right;
}

th:nth-child(8),
td:nth-child(8) {
  width: 220px;
  min-width: 220px;
}

th:nth-child(9),
td:nth-child(9) {
  width: 100px;
  min-width: 100px;
}

th:nth-child(10),
td:nth-child(10) {
  width: 170px;
  min-width: 170px;
}

.remark-cell {
  white-space: normal;
  word-break: break-word;
}

.date-cell {
  white-space: nowrap;
}

.type {
  display: inline-block;
  padding: 4px 8px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: bold;
}

.type.add {
  background: #dcfce7;
  color: #166534;
}

.type.deduct {
  background: #fee2e2;
  color: #991b1b;
}

.type.unlock {
  background: #ede9fe;
  color: #5b21b6;
}

.type.default {
  background: #e5e7eb;
  color: #374151;
}

.empty {
  text-align: center;
  color: #6b7280;
}
</style>