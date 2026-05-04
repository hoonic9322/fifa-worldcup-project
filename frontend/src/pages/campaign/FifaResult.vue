<template>
  <div class="campaign-page">
    <section class="campaign-hero">
      <h1>FIFA Match Results</h1>
      <p>
        Daily FIFA World Cup result updates will be shown here.
      </p>
    </section>

    <section class="campaign-section">
      <h3>Latest Results</h3>

      <table class="result-table">
        <thead>
          <tr>
            <th>Date</th>
            <th>Match</th>
            <th>Result</th>
          </tr>
        </thead>

        <tbody>
          <tr>
            <td>To be updated</td>
            <td>Team A vs Team B</td>
            <td>-</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section class="campaign-section">
      <h3>S9 Promotion</h3>
      <p>
        S9 promotion banner or content can be placed here.
      </p>
    </section>

    <section class="campaign-section">
      <h3>中文说明</h3>
      <p>
        每日 FIFA 比赛结果将在此页面更新。
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

.result-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 20px;
}

.result-table th,
.result-table td {
  padding: 12px;
  border-bottom: 1px solid #e5e7eb;
}
</style>