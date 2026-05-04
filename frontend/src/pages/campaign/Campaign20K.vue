<template>
  <div class="campaign-page">
    <section class="campaign-hero">
      <h1>S9 x 2026 FIFA World Cup</h1>
      <h2>20K Prize Pool</h2>

      <p>
        Join the FIFA World Cup campaign and submit your prediction to participate.
      </p>

      <div class="campaign-actions">
        <router-link to="/login" class="primary-button">
          Member Login
        </router-link>

        <router-link to="/campaign/fifa-schedule" class="secondary-button">
          View FIFA Schedule
        </router-link>
      </div>
    </section>

    <section class="campaign-section">
      <h3>How to Join</h3>

      <div class="step-list">
        <div class="step-card">
          <h4>Step 1</h4>
          <p>Confirm whether you have registered with S9 before.</p>
        </div>

        <div class="step-card">
          <h4>Step 2</h4>
          <p>Fill in your S9 username and phone number correctly.</p>
        </div>

        <div class="step-card">
          <h4>Step 3</h4>
          <p>Answer the FIFA campaign question.</p>
        </div>

        <div class="step-card">
          <h4>Step 4</h4>
          <p>Submit your answer to join the event.</p>
        </div>
      </div>
    </section>

    <section class="campaign-section">
      <h3>中文说明</h3>
      <p>
        参与 S9 2026 FIFA World Cup 活动，填写会员资料并提交预测答案，即可参与 20K 奖池活动。
      </p>
    </section>

    <section class="campaign-section">
      <h3>Terms & Conditions</h3>
      <p>
        Terms and conditions will be updated by the campaign administrator.
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

.step-list {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-top: 20px;
}

.step-card {
  padding: 16px;
  background: #f3f4f6;
  border-radius: 10px;
}

@media (max-width: 768px) {
  .step-list {
    grid-template-columns: 1fr;
  }

  .campaign-actions {
    flex-direction: column;
  }
}
</style>