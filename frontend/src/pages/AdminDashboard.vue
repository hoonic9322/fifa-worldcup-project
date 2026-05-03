<template>
  <div class="admin-dashboard">
    <aside class="sidebar">
      <h2>FIFA Admin</h2>

      <nav>
        <a class="active">Dashboard</a>
        <a>Members</a>
        <a>Answers</a>
        <a>Export</a>
      </nav>
    </aside>

    <main class="main-content">
      <div class="top-bar">
        <div>
          <h1>Admin Dashboard</h1>
          <p>Welcome, {{ adminDisplayName || adminUsername }}</p>
        </div>

        <button type="button" class="logout-button" @click="logout">
          Logout
        </button>
      </div>

      <section class="card-grid">
        <div class="card">
          <h3>Member List</h3>
          <p>Manage member records.</p>
        </div>

        <div class="card">
          <h3>Answer List</h3>
          <p>View submitted answers.</p>
        </div>

        <div class="card">
          <h3>Export Excel</h3>
          <p>Export campaign data.</p>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const adminUsername = ref('')
const adminDisplayName = ref('')

function logout() {
  localStorage.removeItem('adminId')
  localStorage.removeItem('adminUsername')
  localStorage.removeItem('adminDisplayName')

  router.push('/admin/login')
}

onMounted(() => {
  const adminId = localStorage.getItem('adminId')

  if (!adminId) {
    router.push('/admin/login')
    return
  }

  adminUsername.value = localStorage.getItem('adminUsername') || ''
  adminDisplayName.value = localStorage.getItem('adminDisplayName') || ''
})
</script>

<style scoped>
.admin-dashboard {
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

.card-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 18px;
  margin-top: 32px;
}

.card {
  padding: 22px;
  border-radius: 10px;
  background: white;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.06);
}

.card h3 {
  margin: 0 0 10px;
  color: #111827;
}

.card p {
  margin: 0;
  color: #6b7280;
}
</style>