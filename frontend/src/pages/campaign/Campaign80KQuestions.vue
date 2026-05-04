<template>
  <div class="campaign-page">
    <section class="campaign-hero">
      <h1>80K Prize Pool Questions</h1>
      <p>
        Login and unlock FIFA World Cup prediction questions.
      </p>

      <router-link to="/question" class="primary-button">
        Go to Member Questions
      </router-link>
    </section>

    <section class="campaign-section">
      <h3>Question Unlock Rule</h3>
      <p>
        The first question is unlocked by default. Other questions can be unlocked using credit.
      </p>

      <h3>中文说明</h3>
      <p>
        第一题默认解锁，其他题目需要使用积分解锁后才能回答。
      </p>
    </section>
  </div>
</template>

<script setup>
</script>

<style scoped>
.campaign-page {
  min-height: 100vh;
  padding: 40px 20px;
  background: #111827;
  color: #ffffff;
  font-family: Arial, sans-serif;
}

.campaign-hero,
.campaign-section {
  max-width: 960px;
  margin: 0 auto 24px;
  padding: 28px;
  background: #ffffff;
  color: #111827;
  border-radius: 12px;
  text-align: center;
}

.primary-button {
  display: inline-block;
  margin-top: 20px;
  padding: 12px 18px;
  border-radius: 8px;
  text-decoration: none;
  color: #ffffff;
  background: #16a34a;
}
</style>