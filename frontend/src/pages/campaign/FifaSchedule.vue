<template>
  <div class="campaign-page">
    <section class="campaign-hero">
      <h1>FIFA Match Schedule</h1>
      <p>
        Latest FIFA World Cup schedule will be updated here.
      </p>
    </section>

    <section class="campaign-section">
      <h3>Schedule</h3>

      <table class="schedule-table">
        <thead>
          <tr>
            <th>Date</th>
            <th>Match</th>
            <th>Time</th>
            <th>Live TV</th>
          </tr>
        </thead>

        <tbody>
          <tr>
            <td>To be updated</td>
            <td>Team A vs Team B</td>
            <td>--:--</td>
            <td>
              <a href="#" @click.prevent>Live TV Link</a>
            </td>
          </tr>
        </tbody>
      </table>
    </section>

    <section class="campaign-section">
      <h3>中文说明</h3>
      <p>
        最新 FIFA 赛程将在此页面更新。
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

.schedule-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 20px;
}

.schedule-table th,
.schedule-table td {
  padding: 12px;
  border-bottom: 1px solid #e5e7eb;
}
</style>