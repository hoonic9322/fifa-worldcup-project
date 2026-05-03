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

      <div v-else>
        <div
          v-for="question in questions"
          :key="question.id"
          class="question-item"
          :class="{
            'submitted-question': question.hasSubmitted,
            'locked-question': question.isLocked && !question.isUnlocked
          }"
        >
          <div class="question-header">
            <span class="pool-tag">
              {{ question.prizePoolType }}
            </span>

            <span
              v-if="question.hasSubmitted"
              class="submitted-tag"
            >
              Submitted
            </span>

            <span
              v-else-if="question.isLocked && !question.isUnlocked"
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

            <span
              v-else
              class="free-tag"
            >
              Free
            </span>
          </div>

          <h2>
            Question {{ question.id }}:
            {{ question.questionText }}
          </h2>

          <!-- Case 1: This question already submitted -->
          <div v-if="question.hasSubmitted" class="submitted-box">
            <h3>You have submitted this answer</h3>

            <p>
              <strong>Your Answer:</strong>
              {{ question.submittedAnswer || '-' }}
            </p>

            <p>
              <strong>Submitted Time:</strong>
              {{ formatDateTime(question.submittedAt) }}
            </p>

            <p class="submitted-note">
              This answer has been submitted and cannot be edited.
            </p>
          </div>

          <!-- Case 2: This question can answer -->
          <div v-else-if="question.canAnswer" class="answer-area">
            <input
              v-model="answers[question.id]"
              type="text"
              placeholder="Enter your answer"
            />

            <button
              type="button"
              :disabled="submittingQuestionId === question.id"
              @click="submitAnswer(question.id)"
            >
              {{
                submittingQuestionId === question.id
                  ? 'Submitting...'
                  : 'Submit Answer'
              }}
            </button>
          </div>

          <!-- Case 3: This question is locked -->
          <div v-else class="unlock-area">
            <p>This question is locked.</p>
            <p>Deposit RM1,000 to unlock this question.</p>

            <button
              type="button"
              :disabled="unlockingQuestionId === question.id"
              @click="unlockQuestion(question.id)"
            >
              {{
                unlockingQuestionId === question.id
                  ? 'Unlocking...'
                  : 'Unlock Question - 10 Credit'
              }}
            </button>
          </div>
        </div>

        <div v-if="questions.length === 0" class="message-box">
          No questions found.
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
const errorMessage = ref('')
const questions = ref([])
const answers = ref({})

const submittingQuestionId = ref(null)
const unlockingQuestionId = ref(null)

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

async function submitAnswer(questionId) {
  try {
    errorMessage.value = ''

    const answerText = answers.value[questionId]

    if (!answerText || !answerText.trim()) {
      errorMessage.value = 'Please enter your answer.'
      return
    }

    submittingQuestionId.value = questionId

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

    answers.value[questionId] = ''

    await loadQuestions()
  } catch (error) {
    errorMessage.value = error.message || 'Unexpected error occurred.'
  } finally {
    submittingQuestionId.value = null
  }
}

async function unlockQuestion(questionId) {
  try {
    errorMessage.value = ''
    unlockingQuestionId.value = questionId

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
    unlockingQuestionId.value = null
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
  max-width: 920px;
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

.question-item {
  text-align: left;
}

.submitted-question {
  border: 1px solid #bbf7d0;
  background: #ecfdf5;
}

.locked-question {
  opacity: 0.95;
}

.question-header {
  display: flex;
  gap: 10px;
  margin-bottom: 12px;
  flex-wrap: wrap;
}

.pool-tag,
.lock-tag,
.unlock-tag,
.submitted-tag,
.free-tag {
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

.submitted-tag {
  background: #bbf7d0;
  color: #065f46;
}

.free-tag {
  background: #fef3c7;
  color: #92400e;
}

.question-item h2 {
  margin: 0 0 18px;
  font-size: 22px;
  line-height: 1.4;
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

.submitted-box {
  padding: 16px;
  border-radius: 8px;
  background: #ffffff;
  border: 1px solid #bbf7d0;
  color: #111827;
  text-align: center;
}

.submitted-box h3 {
  margin: 0 0 12px;
  color: #065f46;
  font-size: 22px;
  font-weight: 700;
}

.submitted-box p {
  margin: 8px 0;
}

.submitted-note {
  margin-top: 12px;
  color: #6b7280;
  font-size: 14px;
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