<template>
  <div class="campaign-page">
    <section class="campaign-hero">
      <h1>80K Prize Pool</h1>
      <h2>Current Prize Pool: RM88,888</h2>

      <p>
        Login to unlock FIFA World Cup prediction questions.
      </p>

      <div class="campaign-actions">
        <router-link to="/login" class="primary-button">
          Member Login
        </router-link>

        <router-link to="/campaign/80k/how-to-win" class="secondary-button">
          How to Win
        </router-link>
      </div>
    </section>

    <section class="campaign-section">
      <h3>Login Instruction</h3>
      <p>
        Please login using your member username and password. Contact CS if you need assistance.
      </p>

      <h3>中文说明</h3>
      <p>
        请使用会员账号和密码登录。如无法登录，请联系在线客服。
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

.campaign-hero h1 {
  margin: 0 0 8px;
  font-size: 40px;
}

.campaign-hero h2 {
  margin: 0 0 16px;
  font-size: 32px;
  color: #dc2626;
}

.campaign-actions {
  display: flex;
  justify-content: center;
  gap: 12px;
  margin-top: 20px;
}

.primary-button,
.secondary-button {
  display: inline-block;
  padding: 12px 18px;
  border-radius: 8px;
  text-decoration: none;
  color: #ffffff;
}

.primary-button {
  background: #16a34a;
}

.secondary-button {
  background: #2563eb;
}
</style>