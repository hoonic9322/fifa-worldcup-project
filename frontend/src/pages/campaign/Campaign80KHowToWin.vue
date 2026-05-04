<template>
  <div class="campaign-page">
    <section class="campaign-hero">
      <h1>How to Win</h1>
      <h2>80K Prize Pool Distribution</h2>

      <p>
        Prize allocation is based on the number of correct answers.
      </p>

      <router-link to="/campaign/80k/questions" class="primary-button">
        Go to Questions
      </router-link>
    </section>

    <section class="campaign-section">
      <h3>Prize Allocation Rule</h3>

      <table class="rule-table">
        <thead>
          <tr>
            <th>Correct Answers</th>
            <th>Prize Allocation</th>
            <th>Estimated Winning</th>
          </tr>
        </thead>

        <tbody>
          <tr>
            <td>3 and above</td>
            <td>High Tier</td>
            <td>RM XX,XXX</td>
          </tr>
          <tr>
            <td>2 and above</td>
            <td>Middle Tier</td>
            <td>RM X,XXX</td>
          </tr>
          <tr>
            <td>1 and above</td>
            <td>Entry Tier</td>
            <td>RM XXX</td>
          </tr>
        </tbody>
      </table>
    </section>

    <section class="campaign-section">
      <h3>中文说明</h3>
      <p>
        奖金分配将根据会员答对题目的数量计算。答对越多，获得的奖金比例越高。
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

.rule-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 20px;
}

.rule-table th,
.rule-table td {
  padding: 12px;
  border-bottom: 1px solid #e5e7eb;
}
</style>