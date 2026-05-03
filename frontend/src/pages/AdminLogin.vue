<template>
  <div class="admin-login-page">
    <div class="login-card">
      <h1>Admin Login</h1>
      <p class="subtitle">FIFA World Cup Admin Backend</p>

      <div class="form-group">
        <label>Username</label>
        <input
          v-model="username"
          type="text"
          placeholder="Enter admin username"
        />
      </div>

      <div class="form-group">
        <label>Password</label>
        <input
          v-model="password"
          type="password"
          placeholder="Enter admin password"
          @keyup.enter="login"
        />
      </div>

      <button
        type="button"
        class="login-button"
        :disabled="loading"
        @click="login"
      >
        {{ loading ? 'Logging in...' : 'Login' }}
      </button>

      <p v-if="errorMessage" class="error-message">
        {{ errorMessage }}
      </p>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const API_BASE_URL = 'https://localhost:7160'

const username = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref('')

async function login() {
  try {
    errorMessage.value = ''

    if (!username.value.trim()) {
      errorMessage.value = 'Username is required.'
      return
    }

    if (!password.value.trim()) {
      errorMessage.value = 'Password is required.'
      return
    }

    loading.value = true

    const response = await fetch(`${API_BASE_URL}/api/AdminAuth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        username: username.value.trim(),
        password: password.value.trim(),
      }),
    })

    const result = await response.json()

    if (!response.ok || result.status !== 'OK') {
      errorMessage.value = result.message || 'Admin login failed.'
      return
    }

    localStorage.setItem('adminId', result.data.adminId)
    localStorage.setItem('adminUsername', result.data.username)
    localStorage.setItem('adminDisplayName', result.data.displayName)

    router.push('/admin')
  } catch (error) {
    errorMessage.value = error.message || 'Unexpected error occurred.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.admin-login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #111827;
  font-family: Arial, sans-serif;
}

.login-card {
  width: 380px;
  padding: 32px;
  border-radius: 12px;
  background: #ffffff;
  box-shadow: 0 12px 30px rgba(0, 0, 0, 0.25);
}

h1 {
  margin: 0;
  text-align: center;
  color: #111827;
}

.subtitle {
  margin-top: 8px;
  margin-bottom: 28px;
  text-align: center;
  color: #6b7280;
}

.form-group {
  margin-bottom: 18px;
}

label {
  display: block;
  margin-bottom: 6px;
  font-weight: bold;
  color: #374151;
}

input {
  width: 100%;
  padding: 11px 12px;
  box-sizing: border-box;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 14px;
}

.login-button {
  width: 100%;
  margin-top: 8px;
  padding: 12px;
  border: none;
  border-radius: 6px;
  background: #2563eb;
  color: white;
  font-size: 15px;
  font-weight: bold;
  cursor: pointer;
}

.login-button:disabled {
  background: #93c5fd;
  cursor: not-allowed;
}

.error-message {
  margin-top: 16px;
  color: #dc2626;
  text-align: center;
  font-size: 14px;
}
</style>