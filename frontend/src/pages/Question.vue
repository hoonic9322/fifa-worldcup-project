<template>
  <div class="question-page">
    <div class="question-card">
      <h1>FIFA World Cup Question</h1>

      <p class="member-info">
        Member: {{ memberUsername || '-' }} |
        Credit: {{ memberCreditBalance }}
      </p>

      <div v-if="loading" class="message-box">
        Loading questions...
      </div>

      <div v-else-if="errorMessage" class="message-box error">
        {{ errorMessage }}
      </div>

      <div v-else-if="submittedAnswer" class="submitted-box">
        <h2>You have submitted your answer</h2>

        <p>
          <strong>Question ID:</strong>
          {{ submittedAnswer.questionId }}
        </p>

        <p>
          <strong>Your Answer:</strong>
          {{ submittedAnswer.answerText }}
        </p>

        <p>
          <strong>Submitted Time:</strong>
          {{ formatDateTime(submittedAnswer.submittedAt) }}
        </p>
      </div>

      <div v-else>
        <div
          v-for="question in questions"
          :key="question.id"
          class="question-item"
        >
          <div class="question-header">
            <span class="pool-tag">
              {{ question.prizePoolType }}
            </span>

            <span
              v-if="question.isLocked && !question.isUnlocked"
              class="lock-tag"
            >
              Locked
            </span>

            <span
              v-else-if="question.isLocked && question.isUnlocked"
              class="unlock-tag"
            >
              Unlocked
            </span>
          </div>

          <h2>{{ question.questionText }}</h2>

          <div v-if="question.canAnswer" class="answer-area">
            <input
              v-model="answers[question.id]"
              type="text"
              placeholder="Enter your answer"
            />

            <button
              type="button"
              :disabled="submitting"
              @click="submitAnswer(question.id)"
            >
              Submit Answer
            </button>
          </div>

          <div v-else class="unlock-area">
            <p>This question requires credit to unlock.</p>

            <button
              type="button"
              :disabled="unlocking"
              @click="unlockQuestion(question.id)"
            >
              {{ unlocking ? 'Unlocking...' : 'Unlock Question - 10 Credit' }}
            </button>
          </div>
        </div>
      </div>

      <button type="button" class="logout-button" @click="logout">
        Logout
      </button>
    </div>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const API_BASE_URL = 'https://localhost:7160'
const UNLOCK_CREDIT_COST = 10

const loading = ref(true)
const submitting = ref(false)
const unlocking = ref(false)
const errorMessage = ref('')
const questions = ref([])
const answers = ref({})
const submittedAnswer = ref(null)

const memberId = ref(localStorage.getItem('memberId'))
const memberUsername = ref(localStorage.getItem('memberUsername'))
const memberCreditBalance = ref(localStorage.getItem('memberCreditBalance') || 0)

function checkLogin() {
  if (!memberId.value) {
    router.push('/login')
    return false
  }

  return true
}

async function loadQuestions() {
  const response = await fetch(
    `${API_BASE_URL}/api/Question/list?memberId=${memberId.value}`
  )

  const result = await response.json()

  if (!response.ok || result.status !== 'OK') {
    throw new Error(result.message || 'Failed to load questions.')
  }

  questions.value = result.data || []
}

async function loadSubmittedAnswer() {
  const response = await fetch(`${API_BASE_URL}/api/Answer/member/${memberId.value}`)
  const result = await response.json()

  if (!response.ok || result.status !== 'OK') {
    submittedAnswer.value = null
    return
  }

  submittedAnswer.value = result.data
}

async function submitAnswer(questionId) {
  try {
    errorMessage.value = ''

    const answerText = answers.value[questionId]

    if (!answerText || !answerText.trim()) {
      errorMessage.value = 'Please enter your answer.'
      return
    }

    submitting.value = true

    const response = await fetch(`${API_BASE_URL}/api/Answer/submit`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        memberId: Number(memberId.value),
        questionId,
        answerText: answerText.trim(),
      }),
    })

    const result = await response.json()

    if (!response.ok || result.status !== 'OK') {
      errorMessage.value = result.message || 'Failed to submit answer.'
      return
    }

    submittedAnswer.value = result.data
  } catch (error) {
    errorMessage.value = error.message || 'Unexpected error occurred.'
  } finally {
    submitting.value = false
  }
}

async function unlockQuestion(questionId) {
  try {
    errorMessage.value = ''
    unlocking.value = true

    const response = await fetch(`${API_BASE_URL}/api/QuestionUnlock/unlock`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        memberId: Number(memberId.value),
        questionId,
        creditCost: UNLOCK_CREDIT_COST,
      }),
    })

    const result = await response.json()

    if (!response.ok || result.status !== 'OK') {
      errorMessage.value = result.message || 'Failed to unlock question.'
      return
    }

    if (result.data && result.data.balanceAfter !== undefined) {
      memberCreditBalance.value = result.data.balanceAfter
      localStorage.setItem('memberCreditBalance', String(result.data.balanceAfter))
    }

    await loadQuestions()
  } catch (error) {
    errorMessage.value = error.message || 'Unexpected error occurred.'
  } finally {
    unlocking.value = false
  }
}

function logout() {
  localStorage.removeItem('memberId')
  localStorage.removeItem('memberUsername')
  localStorage.removeItem('memberPhoneNumber')
  localStorage.removeItem('memberCreditBalance')

  router.push('/login')
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
    if (!checkLogin()) {
      return
    }

    loading.value = true
    errorMessage.value = ''

    await loadSubmittedAnswer()
    await loadQuestions()
  } catch (error) {
    errorMessage.value = error.message || 'Failed to load question page.'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.question-page {
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding: 48px 20px;
  background: #111827;
  font-family: Arial, sans-serif;
  color: white;
}

.question-card {
  width: 100%;
  max-width: 820px;
  text-align: center;
}

h1 {
  margin: 0 0 12px;
  font-size: 44px;
}

.member-info {
  margin-bottom: 24px;
  color: #d1d5db;
}

.message-box,
.submitted-box,
.question-item {
  margin-top: 20px;
  padding: 24px;
  border-radius: 10px;
  background: white;
  color: #111827;
}

.error {
  color: #dc2626;
}

.submitted-box {
  background: #ecfdf5;
  border: 1px solid #bbf7d0;
}

.question-item {
  text-align: left;
}

.question-header {
  display: flex;
  gap: 10px;
  margin-bottom: 12px;
}

.pool-tag,
.lock-tag,
.unlock-tag {
  display: inline-block;
  padding: 5px 10px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: bold;
}

.pool-tag {
  background: #dbeafe;
  color: #1d4ed8;
}

.lock-tag {
  background: #fee2e2;
  color: #991b1b;
}

.unlock-tag {
  background: #dcfce7;
  color: #166534;
}

.question-item h2 {
  margin: 0 0 18px;
  font-size: 22px;
}

.answer-area {
  display: flex;
  gap: 12px;
}

.answer-area input {
  flex: 1;
}

input {
  padding: 11px 12px;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 15px;
}

button {
  padding: 11px 16px;
  border: none;
  border-radius: 6px;
  background: #2563eb;
  color: white;
  cursor: pointer;
  font-size: 15px;
}

button:disabled {
  background: #93c5fd;
  cursor: not-allowed;
}

.unlock-area {
  padding: 16px;
  border-radius: 8px;
  background: #f9fafb;
  text-align: center;
}

.unlock-area p {
  margin: 0 0 12px;
  color: #6b7280;
}

.logout-button {
  margin-top: 24px;
  background: #dc2626;
}
</style>