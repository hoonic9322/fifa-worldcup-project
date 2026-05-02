<template>
  <div class="page">
    <h1>Member Login</h1>

    <form class="login-form" @submit.prevent="handleLogin">
      <label>
        Username
        <input v-model="username" type="text" placeholder="Enter username" />
      </label>

      <label>
        Password
        <input v-model="password" type="password" placeholder="Enter password" />
      </label>

      <button type="submit" :disabled="isLoading">
        {{ isLoading ? 'Logging in...' : 'Login' }}
      </button>
    </form>

    <p v-if="message" class="message">{{ message }}</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { API_BASE_URL } from '../config/api'

const router = useRouter()

const username = ref('')
const password = ref('')
const message = ref('')
const isLoading = ref(false)

async function handleLogin() {
  message.value = ''

  if (!username.value || !password.value) {
    message.value = 'Please enter username and password.'
    return
  }

  try {
    isLoading.value = true

    const response = await fetch(`${API_BASE_URL}/api/MemberAuth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        username: username.value,
        password: password.value,
      }),
    })

    const result = await response.json()

    if (!response.ok) {
      message.value = result.message || 'Login failed.'
      return
    }

    localStorage.setItem('memberId', result.member.id)
    localStorage.setItem('memberUsername', result.member.username)
    localStorage.setItem('memberPhoneNumber', result.member.phoneNumber || '')
    localStorage.setItem('memberCreditBalance', result.member.creditBalance)

    router.push('/question')
  } catch (error) {
    message.value = 'Unable to connect to server. Please try again.'
  } finally {
    isLoading.value = false
  }
}
</script>

<style scoped>
.page {
  max-width: 420px;
  margin: 80px auto;
  padding: 24px;
}

h1 {
  text-align: center;
  margin-bottom: 24px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

label {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-weight: 600;
}

input {
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 6px;
}

button {
  padding: 12px;
  background: #16a34a;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

button:disabled {
  background: #9ca3af;
  cursor: not-allowed;
}

.message {
  margin-top: 16px;
  color: #dc2626;
  text-align: center;
}
</style>