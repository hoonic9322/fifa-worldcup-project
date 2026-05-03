<template>
  <div class="admin-layout">
    <aside class="sidebar">
      <h2>FIFA Admin</h2>

      <nav>
        <router-link to="/admin">Dashboard</router-link>
        <router-link to="/admin/members" class="active">Members</router-link>
        <router-link to="/admin/answers">Answers</router-link>
        <router-link to="/admin/export">Export</router-link>
      </nav>
    </aside>

    <main class="main-content">
      <div class="top-bar">
        <div>
          <h1>Member List</h1>
          <p>View registered members and answer status.</p>
        </div>

        <button type="button" class="logout-button" @click="logout">
          Logout
        </button>
      </div>

      <div v-if="loading" class="card">
        Loading member list...
      </div>

      <div v-else-if="errorMessage" class="card error">
        {{ errorMessage }}
      </div>

      <div v-else>
        <div class="filter-card">
          <div class="search-row">
            <input
              v-model="keyword"
              type="text"
              placeholder="Search by username or phone number"
              @keyup.enter="searchMembers"
            />

            <button type="button" class="search-button" @click="searchMembers">
              Search
            </button>

            <button type="button" class="reset-button" @click="resetSearch">
              Reset
            </button>
          </div>
        </div>

        <div class="table-card">
          <table>
            <thead>
              <tr>
                <th>Member ID</th>
                <th>Username</th>
                <th>Phone Number</th>
                <th>Credit Balance</th>
                <th>Answered Count</th>
                <th>Last Answer Time</th>
                <th>Created At</th>
              </tr>
            </thead>

            <tbody>
              <tr v-if="members.length === 0">
                <td colspan="7" class="empty">
                  No members found.
                </td>
              </tr>

              <tr v-for="member in members" :key="member.memberId">
                <td>{{ member.memberId }}</td>
                <td>{{ member.username }}</td>
                <td>{{ member.phoneNumber || '-' }}</td>
                <td>{{ member.creditBalance }}</td>
                <td>{{ member.answeredCount }}</td>
                <td>{{ formatDateTime(member.lastAnswerTime) }}</td>
                <td>{{ formatDateTime(member.createdAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const API_BASE_URL = 'https://localhost:7160'

const loading = ref(true)
const errorMessage = ref('')
const members = ref([])
const keyword = ref('')

function checkAdminLogin() {
  const adminId = localStorage.getItem('adminId')

  if (!adminId) {
    router.push('/admin/login')
    return false
  }

  return true
}

async function loadMembers(searchKeyword = '') {
  const query = searchKeyword
    ? `?keyword=${encodeURIComponent(searchKeyword)}`
    : ''

  const response = await fetch(`${API_BASE_URL}/api/AdminMember/list${query}`)
  const result = await response.json()

  if (!response.ok || result.status !== 'OK') {
    throw new Error(result.message || 'Failed to load member list.')
  }

  members.value = result.data || []
}

async function searchMembers() {
  try {
    loading.value = true
    errorMessage.value = ''

    await loadMembers(keyword.value.trim())
  } catch (error) {
    errorMessage.value = error.message || 'Failed to search member list.'
  } finally {
    loading.value = false
  }
}

async function resetSearch() {
  keyword.value = ''

  try {
    loading.value = true
    errorMessage.value = ''

    await loadMembers()
  } catch (error) {
    errorMessage.value = error.message || 'Failed to reset member list.'
  } finally {
    loading.value = false
  }
}

function logout() {
  localStorage.removeItem('adminId')
  localStorage.removeItem('adminUsername')
  localStorage.removeItem('adminDisplayName')

  router.push('/admin/login')
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
  try {
    if (!checkAdminLogin()) {
      return
    }

    loading.value = true
    errorMessage.value = ''

    await loadMembers()
  } catch (error) {
    errorMessage.value = error.message || 'Failed to load member list.'
  } finally {
    loading.value = false
  }
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
  padding: 32px;
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
.table-card {
  margin-top: 28px;
  padding: 20px;
  border-radius: 10px;
  background: white;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
}

.error {
  color: #dc2626;
}

.filter-card {
  margin-top: 28px;
  padding: 18px 20px;
  border-radius: 10px;
  background: white;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
}

.search-row {
  display: flex;
  gap: 12px;
}

.search-row input {
  flex: 1;
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
}

.search-button,
.reset-button {
  padding: 10px 16px;
  border: none;
  border-radius: 6px;
  color: white;
  cursor: pointer;
}

.search-button {
  background: #2563eb;
}

.reset-button {
  background: #6b7280;
}

table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

th {
  padding: 12px;
  background: #f9fafb;
  border-bottom: 1px solid #e5e7eb;
  text-align: left;
  color: #374151;
}

td {
  padding: 12px;
  border-bottom: 1px solid #e5e7eb;
  color: #111827;
}

.empty {
  text-align: center;
  color: #6b7280;
}
</style>