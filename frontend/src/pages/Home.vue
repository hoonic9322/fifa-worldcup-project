<template>
  <div class="page">
    <h1>FIFA World Cup Campaign</h1>
    <p>Join the prize pool campaign and answer World Cup questions.</p>

    <div class="actions">
      <RouterLink to="/login" class="button">Login to Join</RouterLink>
    </div>
  </div>
</template>

<style scoped>
.page {
  max-width: 800px;
  margin: 80px auto;
  padding: 24px;
  text-align: center;
}

h1 {
  font-size: 36px;
  margin-bottom: 16px;
}

p {
  font-size: 18px;
  margin-bottom: 24px;
}

.button {
  display: inline-block;
  padding: 12px 24px;
  background: #1d4ed8;
  color: white;
  text-decoration: none;
  border-radius: 8px;
}
</style>