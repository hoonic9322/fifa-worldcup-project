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

      <button type="submit">Login</button>
    </form>

    <p v-if="message" class="message">{{ message }}</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const username = ref('')
const password = ref('')
const message = ref('')

function handleLogin() {
  if (!username.value || !password.value) {
    message.value = 'Please enter username and password.'
    return
  }

  // Temporary login for MVP frontend test
  localStorage.setItem('memberUsername', username.value)
  router.push('/question')
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

.message {
  margin-top: 16px;
  color: #dc2626;
  text-align: center;
}
</style>