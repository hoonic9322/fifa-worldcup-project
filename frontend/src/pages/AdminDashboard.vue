<template>
  <div class="admin-page">
    <aside class="admin-sidebar">
      <h2 class="admin-logo">FIFA Admin</h2>

      <nav class="admin-nav">
        <router-link to="/admin/dashboard" class="admin-nav-link">
          Dashboard
        </router-link>
        <router-link to="/admin/members" class="admin-nav-link">
          Members
        </router-link>
        <router-link to="/admin/answers" class="admin-nav-link">
          Answers
        </router-link>
        <router-link to="/admin/credit" class="admin-nav-link">
          Credit
        </router-link>
        <router-link to="/admin/export" class="admin-nav-link">
          Export
        </router-link>
      </nav>
    </aside>

    <main class="admin-main">
      <div class="admin-header">
        <div>
          <h1>Admin Dashboard</h1>
          <p>Welcome, System Admin</p>
        </div>

        <button class="logout-button" @click="logout">
          Logout
        </button>
      </div>

      <section v-if="loading" class="dashboard-message-card">
        Loading dashboard summary...
      </section>

      <section v-else-if="errorMessage" class="dashboard-message-card error">
        {{ errorMessage }}
      </section>

      <section v-else class="dashboard-grid">
        <div class="dashboard-card">
          <h3>Total Members</h3>
          <p class="dashboard-number">{{ summary.totalMembers }}</p>
          <span>Registered member records.</span>
        </div>

        <div class="dashboard-card">
          <h3>Total Questions</h3>
          <p class="dashboard-number">{{ summary.totalQuestions }}</p>
          <span>Campaign question records.</span>
        </div>

        <div class="dashboard-card">
          <h3>Submitted Answers</h3>
          <p class="dashboard-number">{{ summary.totalSubmittedAnswers }}</p>
          <span>Member submitted answers.</span>
        </div>

        <div class="dashboard-card">
          <h3>Unlock Records</h3>
          <p class="dashboard-number">{{ summary.totalUnlockRecords }}</p>
          <span>Question unlock records.</span>
        </div>

        <div class="dashboard-card">
          <h3>Credit Transactions</h3>
          <p class="dashboard-number">{{ summary.totalCreditTransactions }}</p>
          <span>Add, deduct, and unlock records.</span>
        </div>
      </section>
    </main>
  </div>
</template>

<script>
export default {
  name: "AdminDashboard",

  data() {
    return {
      loading: false,
      errorMessage: "",
      summary: {
        totalMembers: 0,
        totalQuestions: 0,
        totalSubmittedAnswers: 0,
        totalUnlockRecords: 0,
        totalCreditTransactions: 0
      }
    };
  },

  mounted() {
    this.loadDashboardSummary();
  },

  methods: {
    async loadDashboardSummary() {
      this.loading = true;
      this.errorMessage = "";

      try {
        const response = await fetch(
          "https://localhost:7160/api/admin/dashboard/summary"
        );

        const result = await response.json();

        if (!response.ok || result.status !== "OK") {
          throw new Error(result.message || "Failed to load dashboard summary.");
        }

        this.summary = result.data;
      } catch (error) {
        this.errorMessage =
          error.message || "Failed to load dashboard summary.";
      } finally {
        this.loading = false;
      }
    },

    logout() {
  localStorage.removeItem('adminId')
  localStorage.removeItem('adminUsername')
  localStorage.removeItem('adminDisplayName')

  this.$router.push('/admin/login')
}
  }
};
</script>

<style scoped>
.admin-page {
  display: flex;
  min-height: 100vh;
  background: #f3f4f6;
  color: #111827;
}

.admin-sidebar {
  width: 300px;
  min-height: 100vh;
  background: #111827;
  color: #ffffff;
  padding: 24px;
  box-sizing: border-box;
}

.admin-logo {
  font-size: 28px;
  font-weight: 700;
  text-align: center;
  margin: 0 0 32px 0;
}

.admin-nav {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.admin-nav-link {
  color: #ffffff;
  text-decoration: none;
  font-size: 22px;
  padding: 14px 20px;
  border-radius: 8px;
  text-align: center;
}

.admin-nav-link.router-link-active,
.admin-nav-link:hover {
  background: #1f2937;
}

.admin-main {
  flex: 1;
  padding: 24px 40px;
  box-sizing: border-box;
}

.admin-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 40px;
}

.admin-header h1 {
  font-size: 64px;
  line-height: 1;
  margin: 0;
  color: #111827;
}

.admin-header p {
  font-size: 22px;
  color: #6b7280;
  margin: 0;
  text-align: center;
}

.logout-button {
  background: #dc2626;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  padding: 14px 24px;
  font-size: 16px;
  cursor: pointer;
}

.logout-button:hover {
  background: #b91c1c;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(220px, 1fr));
  gap: 24px;
}

.dashboard-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 28px 24px;
  text-align: center;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.06);
}

.dashboard-card h3 {
  font-size: 24px;
  margin: 0 0 12px 0;
  color: #111827;
}

.dashboard-number {
  font-size: 42px;
  font-weight: 700;
  margin: 0 0 10px 0;
  color: #111827;
}

.dashboard-card span {
  font-size: 18px;
  color: #6b7280;
}

.dashboard-message-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 24px;
  font-size: 20px;
  text-align: center;
  color: #374151;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.06);
}

.dashboard-message-card.error {
  color: #dc2626;
}
</style>