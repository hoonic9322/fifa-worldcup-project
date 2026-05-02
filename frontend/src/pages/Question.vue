<template>
  <div class="page">
    <h1>World Cup Question</h1>

    <p class="welcome">Welcome, {{ username }}</p>

    <div class="question-box">
      <h2>Question 1</h2>
      <p>Which country do you think will win the next match?</p>

      <textarea
        v-model="answer"
        placeholder="Enter your answer"
      ></textarea>

      <button @click="submitAnswer">Submit Answer</button>
    </div>

    <p v-if="message" class="message">{{ message }}</p>

    <RouterLink to="/" class="back-link">Back to Home</RouterLink>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const username = localStorage.getItem('memberUsername') || 'Guest'
const answer = ref('')
const message = ref('')

function submitAnswer() {
  if (!answer.value) {
    message.value = 'Please enter your answer.'
    return
  }

  // Temporary frontend-only submission
  message.value = 'Answer submitted successfully.'
}
</script>

<style scoped>
.page {
  max-width: 700px;
  margin: 60px auto;
  padding: 24px;
}

h1 {
  text-align: center;
  margin-bottom: 12px;
}

.welcome {
  text-align: center;
  margin-bottom: 24px;
}

.question-box {
  border: 1px solid #ddd;
  border-radius: 10px;
  padding: 24px;
}

textarea {
  width: 100%;
  min-height: 120px;
  margin-top: 12px;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 6px;
}

button {
  margin-top: 16px;
  padding: 12px 20px;
  background: #dc2626;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.message {
  margin-top: 16px;
  color: #16a34a;
  text-align: center;
}

.back-link {
  display: block;
  margin-top: 24px;
  text-align: center;
}
</style>