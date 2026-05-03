<template>
  <div class="admin-layout">
    <aside class="sidebar">
      <h2>FIFA Admin</h2>

      <nav>
        <router-link to="/admin">Dashboard</router-link>
        <router-link to="/admin/members">Members</router-link>
        <router-link to="/admin/answers" class="active">Answers</router-link>
         <router-link to="/admin/credit">Credit</router-link>
        <router-link to="/admin/export">Export</router-link>
      </nav>
    </aside>

    <main class="main-content">
      <div class="top-bar">
        <div>
          <h1>Answer List</h1>
          <p>View submitted member answers.</p>
        </div>

        <button type="button" class="logout-button" @click="logout">
          Logout
        </button>
      </div>

      <div v-if="loading" class="card">
        Loading answer list...
      </div>

      <div v-else-if="errorMessage" class="card error">
        {{ errorMessage }}
      </div>

      <div v-else class="table-card">
        <table>
          <thead>
            <tr>
              <th>Answer ID</th>
              <th>Username</th>
              <th>Phone Number</th>
              <th>Prize Pool</th>
              <th>Question</th>
              <th>Answer</th>
              <th>Submitted Time</th>
            </tr>
          </thead>

          <tbody>
            <tr v-if="answers.length === 0">
              <td colspan="7" class="empty">
                No answers found.
              </td>
            </tr>

            <tr v-for="answer in answers" :key="answer.answerId">
              <td>{{ answer.answerId }}</td>
              <td>{{ answer.username }}</td>
              <td>{{ answer.phoneNumber || '-' }}</td>
              <td>{{ answer.prizePoolType }}</td>
              <td class="question-text">{{ answer.questionText }}</td>
              <td>{{ answer.answerText }}</td>
              <td>{{ formatDateTime(answer.submittedAt) }}</td>
            </tr>
          </tbody>
        </table>
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
const answers = ref([])

function checkAdminLogin() {
  const adminId = localStorage.getItem('adminId')

  if (!adminId) {
    router.push('/admin/login')
    return false
  }

  return true
}

async function loadAnswers() {
  const response = await fetch(`${API_BASE_URL}/api/AdminAnswer/list`)
  const result = await response.json()

  if (!response.ok || result.status !== 'OK') {
    throw new Error(result.message || 'Failed to load answer list.')
  }

  answers.value = result.data || []
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

    await loadAnswers()
  } catch (error) {
    errorMessage.value = error.message || 'Failed to load answer list.'
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
  vertical-align: top;
}

.question-text {
  max-width: 360px;
}

.empty {
  text-align: center;
  color: #6b7280;
}
</style>