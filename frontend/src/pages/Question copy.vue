<template>
  <div class="question-page">
    <h1>FIFA World Cup Question</h1>

    <div v-if="loading" class="card">
      Loading...
    </div>

    <div v-else-if="errorMessage" class="card error">
      {{ errorMessage }}
    </div>

    <div v-else>
      <!-- Submitted Answer View -->
      <div v-if="submittedAnswer" class="card submitted-box">
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

      <!-- Answer Form -->
      <div v-else class="card">
        <h2>Question List</h2>

        <div v-if="questions.length === 0">
          No questions found.
        </div>

        <div
          v-for="question in questions"
          :key="question.id"
          class="question-item"
        >
          <p>
            <strong>{{ question.prizePoolType }}</strong>
          </p>

          <p>{{ question.questionText }}</p>

          <input
            v-model="answers[question.id]"
            type="text"
            placeholder="Enter your answer"
            class="answer-input"
          />

          <button
            type="button"
            class="submit-button"
            @click="submitAnswer(question.id)"
          >
            Submit Answer
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const API_BASE_URL = 'https://localhost:7160'

const loading = ref(true)
const errorMessage = ref('')
const questions = ref([])
const submittedAnswer = ref(null)
const answers = reactive({})

function getMemberId() {
  const directMemberId = localStorage.getItem('memberId')

  if (directMemberId) {
    return Number(directMemberId)
  }

  const memberInfo = localStorage.getItem('memberInfo')

  if (memberInfo) {
    try {
      const parsedMemberInfo = JSON.parse(memberInfo)
      return Number(parsedMemberInfo.id || parsedMemberInfo.memberId)
    } catch {
      return 0
    }
  }

  return 0
}

async function loadQuestions() {
  const response = await fetch(`${API_BASE_URL}/api/Question/list`)

  const result = await response.json()

  if (!response.ok || result.status !== 'OK') {
    throw new Error(result.message || 'Failed to load question list.')
  }

  questions.value = result.data || []
}

async function loadSubmittedAnswer(memberId) {
  const response = await fetch(`${API_BASE_URL}/api/Answer/member/${memberId}`)

  const result = await response.json()

  if (!response.ok || result.status !== 'OK') {
    throw new Error(result.message || 'Failed to load submitted answer.')
  }

  submittedAnswer.value = result.data
}

async function submitAnswer(questionId) {
  try {
    const memberId = getMemberId()

    if (!memberId) {
      alert('Please login first.')
      router.push('/login')
      return
    }

    const answerText = answers[questionId]

    if (!answerText || !answerText.trim()) {
      alert('Please enter your answer.')
      return
    }

    const response = await fetch(`${API_BASE_URL}/api/Answer/submit`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        memberId,
        questionId,
        answerText: answerText.trim()
      })
    })

    const result = await response.json()

    if (!response.ok || result.status !== 'OK') {
      alert(result.message || 'Failed to submit answer.')
      return
    }

    alert('Answer submitted successfully.')

    await loadSubmittedAnswer(memberId)
  } catch (error) {
    alert(error.message || 'Unexpected error occurred.')
  }
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
    loading.value = true
    errorMessage.value = ''

    const memberId = getMemberId()

    if (!memberId) {
      router.push('/login')
      return
    }

    await loadQuestions()
    await loadSubmittedAnswer(memberId)
  } catch (error) {
    errorMessage.value = error.message || 'Failed to load page.'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.question-page {
  max-width: 900px;
  margin: 40px auto;
  padding: 20px;
  font-family: Arial, sans-serif;
}

.card {
  margin-top: 20px;
  padding: 20px;
  border: 1px solid #dddddd;
  border-radius: 8px;
  background: #ffffff;
}

.error {
  color: #b00020;
  border-color: #b00020;
}

.submitted-box {
  background: #f0fff4;
  border-color: #8fd19e;
}

.question-item {
  margin-top: 20px;
  padding: 16px;
  border: 1px solid #eeeeee;
  border-radius: 6px;
}

.answer-input {
  width: 100%;
  padding: 10px;
  margin-top: 10px;
  box-sizing: border-box;
}

.submit-button {
  margin-top: 12px;
  padding: 10px 16px;
  border: none;
  border-radius: 4px;
  background: #1976d2;
  color: white;
  cursor: pointer;
}

.submit-button:hover {
  background: #125aa0;
}
</style>