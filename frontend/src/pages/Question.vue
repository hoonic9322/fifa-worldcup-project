<template>
  <div class="page">
    <h1>World Cup Questions</h1>

    <p class="welcome">Welcome, {{ username }}</p>

    <div v-if="isLoading" class="loading">
      Loading questions...
    </div>

    <div v-else-if="errorMessage" class="error">
      {{ errorMessage }}
    </div>

    <div v-else>
      <div
        v-for="question in questions"
        :key="question.id"
        class="question-box"
      >
        <div class="question-header">
          <span class="pool-type">{{ question.prizePoolType }}</span>
          <span v-if="question.isLocked" class="locked">Locked</span>
          <span v-else class="unlocked">Unlocked</span>
        </div>

        <h2>Question {{ question.id }}</h2>
        <p>{{ question.questionText }}</p>

        <textarea
          v-model="answers[question.id]"
          placeholder="Enter your answer"
          :disabled="question.isLocked"
        ></textarea>

        <button
          @click="submitAnswer(question)"
          :disabled="question.isLocked"
        >
          Submit Answer
        </button>
      </div>
    </div>

    <p v-if="message" class="message">{{ message }}</p>

    <RouterLink to="/" class="back-link">Back to Home</RouterLink>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { API_BASE_URL } from '../config/api'

const username = localStorage.getItem('memberUsername') || 'Guest'

const questions = ref([])
const answers = ref({})
const message = ref('')
const errorMessage = ref('')
const isLoading = ref(false)

async function loadQuestions() {
  try {
    isLoading.value = true
    errorMessage.value = ''

    const response = await fetch(`${API_BASE_URL}/api/Question/list`)
    const result = await response.json()

    if (!response.ok) {
      errorMessage.value = result.message || 'Failed to load questions.'
      return
    }

    questions.value = result.data || []
  } catch (error) {
    errorMessage.value = 'Unable to connect to server. Please try again.'
  } finally {
    isLoading.value = false
  }
}

function submitAnswer(question) {
  const answer = answers.value[question.id]

  if (!answer) {
    message.value = 'Please enter your answer.'
    return
  }

  // Temporary frontend-only submission.
  // Real submit API will be added in next step.
  message.value = `Answer for Question ${question.id} saved locally for testing.`
}

onMounted(() => {
  loadQuestions()
})
</script>

<style scoped>
.page {
  max-width: 800px;
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

.loading,
.error {
  text-align: center;
  margin: 24px 0;
}

.error {
  color: #dc2626;
}

.question-box {
  border: 1px solid #ddd;
  border-radius: 10px;
  padding: 24px;
  margin-bottom: 20px;
}

.question-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
}

.pool-type {
  font-weight: 700;
  color: #1d4ed8;
}

.locked {
  color: #dc2626;
  font-weight: 700;
}

.unlocked {
  color: #16a34a;
  font-weight: 700;
}

textarea {
  width: 100%;
  min-height: 100px;
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

button:disabled {
  background: #9ca3af;
  cursor: not-allowed;
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